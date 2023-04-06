using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DbBackuper.Core.AWS;
using DbBackuper.Core.LeaseWeb;
using DbBackuper.Core.Models;
using DbBackuper.Core.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace DbBackuper;

public class Worker
{
    private readonly IOptions<WorkerSettings> _workerSettings;
    private readonly ILogger _logger;

    //Sometimes sftp server is not started fast enough after VPS start?so lets retry   
    const int retryDelayInSec = 30;
    const int backupDays = 14;
    const int numberOfAttemptsToStartSftpByDefault = 10;
    const int numberOfAttemptsToStopSftpByDefault = 10;
    const string allBackups = "*";

    private WorkerSettings Settings => _workerSettings.Value;

    public Worker(IOptions<WorkerSettings> workerSettings, ILogger<Worker> logger)
    {
        _workerSettings = workerSettings;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("base dir: " + AppContext.BaseDirectory);

        var report = Settings.UseAWS
            ? await ProcessAWSUpload()
            : await ProcessFTPUpload();

        //EmailReport
        SendEmailReport(report);

        Environment.Exit(0);
    }

    private async Task<WorkerReport> ProcessFTPUpload()
    {
        var report = new WorkerReport
        {
            DatabaseReports = new List<DatabaseReport>(),
            UploadType = UploadType.SFTP
        };
        var vsClient = new VirtualServerApiClient(Settings.ApiKey!, Settings.ApiRoot);
        _logger.LogInformation("Starting VPS.");
        (bool Result, string InitialVpsState, string? LastError) runResult = await vsClient.TryRunVpsAsync(Settings.VpsId,
            Settings.WaitVpsStatusRetryCount, Settings.WaitVpsStatusRetryDelaySec);
        report.RunResult = runResult;

        if (runResult.Result)
        {
            //TODO: implement database clearing    
            var databaseReport = DbReport();
            await CleanOldFilesSftp(databaseReport);

            foreach (var databaseSetting in Settings.DatabaseSettings)
            {
                var dbReport = await BackupDatabaseAndUploadToFtp(databaseSetting);
                report.DatabaseReports.Add(dbReport);
            }
        }

        if (Settings.StopVpsOnFinish)
        {
            _logger.LogInformation("Stopping VPS.");
            var stopResult = await vsClient.TryStopVpsAsync(Settings.VpsId,
                RetryAttemptsStopSftp(), Settings.WaitVpsStatusRetryDelaySec);
            report.StopResult = stopResult;
        }

        return report;
    }

    public async Task<WorkerReport> ProcessAWSUpload()
    {
        var report = new WorkerReport
        {
            DatabaseReports = new List<DatabaseReport>(),
            UploadType = UploadType.AWS,
        };

        foreach (var databaseSetting in Settings.DatabaseSettings)
        {
            var dbReport = await BackupDatabaseAndUploadToAWS(databaseSetting);
            report.DatabaseReports.Add(dbReport);
        }

        return report;
    }

    private DatabaseReport BackupDatabase(DatabaseSetting databaseSetting)
    {
        var report = DbReport(databaseSetting);

        //Backup DB to temp folder
        try
        {
            report.BackupTempLocation = DbBackupsProvider.BackupDatabase(
                databaseSetting.DatabaseName,
                databaseSetting.UserName,
                databaseSetting.UserPassword,
                databaseSetting.ServerName,
                Settings.BackupsTempFolder);

            report.IsBackupCreated = true;
            report.BackupName = Path.GetFileName(report.BackupTempLocation);

            _logger.LogInformation($"Backup '{report.BackupName}' is created.");
        }
        catch (Exception ex)
        {
            report.LastError = ex.Message;
            _logger.LogError(ex, ex.Message);
        }
        return report;
    }

    private void CleanBackupTempFile(ref DatabaseReport report)
    {
        if (!report.IsBackupUploaded)
            return;
        try
        {
            //Delete file
            File.Delete(report.BackupTempLocation!);
            _logger.LogInformation($"Delete file '{report.BackupName}' is completed.");
            report.IsTempBackupDeleted = true;
        }
        catch (Exception ex)
        {
            report.LastError = ex.Message;
            _logger.LogError(ex, ex.Message);
        }
    }

    private async Task<DatabaseReport> BackupDatabaseAndUploadToFtp(DatabaseSetting databaseSetting)
    {
        var report = BackupDatabase(databaseSetting);
        //Copy backup to FTP
        if (report.IsBackupCreated)
        {
            await UploadBackupAsync(report);
        }

        CleanBackupTempFile(ref report);

        return report;
    }

    private async Task<DatabaseReport> BackupDatabaseAndUploadToAWS(DatabaseSetting databaseSetting)
    {
        var report = BackupDatabase(databaseSetting);
        if (report.IsBackupCreated)
        {
            var uploader = new AWSUploader(_workerSettings);
            report = await uploader.UploadToAWS(report);
        }

        CleanBackupTempFile(ref report);

        return report;
    }

    private async Task UploadBackupAsync(DatabaseReport report)
    {
        //Sometimes sftp server is not started fast enough after VPS start?so lets retry 
        int retryCount = RetryAttemptsStartSftp();
        const int retryDelayInSec = 30;
        var retryNumber = 0;
        while (true)
        {
            try
            {
                Renci.SshNet.SftpClient client = GetClientAndConnect();
                await using var sendStream = File.OpenRead(report.BackupTempLocation);
                client.UploadFile(sendStream, report.BackupName, true);
                client.Disconnect();

                report.IsBackupUploaded = true;
                _logger.LogInformation($"Upload file '{report.BackupName}' is completed.");
                return;
            }
            catch (Exception ex)
            {
                if (retryNumber++ >= retryCount)
                {
                    report.LastError = ex.Message;
                    _logger.LogError(ex, ex.Message);
                    return;
                }

                await Task.Delay(retryDelayInSec);
            }
        }
    }

    private DatabaseReport DbReport(DatabaseSetting databaseSetting = null)
        => new DatabaseReport { DatabaseSetting = databaseSetting, };

    private int RetryAttemptsStartSftp()
        => Settings.NumberOfAttemptsToStartSftp ?? numberOfAttemptsToStartSftpByDefault;

    private int RetryAttemptsStopSftp()
        => Settings.NumberOfAttemptsToStopSftp ?? numberOfAttemptsToStopSftpByDefault;

    //TODO: implement and test implementation
    private async Task CleanOldFilesSftp(DatabaseReport report)
    {
        int retryCount = RetryAttemptsStartSftp();
        var dbRetentionPeriod = Settings.BackupDays ?? backupDays;
        List<string> dbMasksList = !(string.IsNullOrEmpty(Settings.BackupMask)) ? Settings.BackupMask.Split(',').Select(t => t.Trim()).ToList() : new List<string> { allBackups };

        var retryNumber = 0;
        while (true)
        {
            try
            {
                Renci.SshNet.SftpClient client = GetClientAndConnect();
                var files = client.ListDirectory("");

                
                    foreach (var file in files)
                    {
                        if (FitsOneOfMultipleMasks(file.Name, dbMasksList) && file.LastWriteTime.AddDays(dbRetentionPeriod) < DateTime.Now)
                        {
                             file.Delete();
                            _logger.LogInformation($"Delete old file '{file.FullName}' is completed.");
                        }
                    }
                

                client.Disconnect();

                return;
            }
            catch (Exception ex)
            {
                if (retryNumber++ >= retryCount)
                {
                    report.LastError = ex.Message;
                    _logger.LogError(ex, ex.Message);

                    return;
                }

                await Task.Delay(retryDelayInSec);
            }
        }
    }

    private bool FitsMask(string sFileName, string sFileMask)
    {
        String convertedMask = "^" + Regex.Escape(sFileMask).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        Regex regexMask = new Regex(convertedMask, RegexOptions.IgnoreCase);
       
        return regexMask.IsMatch(sFileName);
    }
    private bool FitsOneOfMultipleMasks(string fileName, List<string> fileMasks)
    {
        return fileMasks
            .Any(fileMask => FitsMask(fileName, fileMask));
    }

    private Renci.SshNet.SftpClient GetClientAndConnect()
    {
        var authMethod = new Renci.SshNet.PasswordAuthenticationMethod(Settings.SftpUserName, Settings.SftpUserPassword);
        var connectionInfo = new Renci.SshNet.ConnectionInfo(Settings.VpsIp, Settings.SftpUserName, authMethod);
        var client = new Renci.SshNet.SftpClient(connectionInfo);
        client.Connect();

        return client;
    }

    private void SendEmailReport(WorkerReport report)
    {
        var ms = Settings.MailSettings;
        if (!ms.SendEmailReport) return;

        _logger.LogInformation($"Sending email report to {ms.MailTo}.");

        try
        {
            using var mm = new MailMessage();
            mm.From = new MailAddress(ms.MailFrom, ms.DisplayNameFrom);
            foreach (var mailTo in ms.MailTo.Split(';'))
            {
                mm.To.Add(mailTo);
            }

            mm.Subject = ms.Subject;
            mm.Body = PrepareReportBody(report);
            mm.IsBodyHtml = false;

            using var smtp = new SmtpClient();
            smtp.Host = ms.Host;
            smtp.Port = ms.Port;
            smtp.EnableSsl = true;
            var networkCred = new NetworkCredential(ms.User, ms.Password);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = networkCred;

            smtp.Send(mm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    private string PrepareReportBody(WorkerReport report)
    {
        var result = new StringBuilder();
        if (report.UploadType == UploadType.SFTP)
            result.AppendLine(GetSftpVpsStates(report));

        foreach (var databaseReport in report.DatabaseReports)
        {
            result.AppendLine();
            result.AppendLine($"{databaseReport.DatabaseSetting.DatabaseName}:");
            if (databaseReport.IsBackupCreated)
            {
                result.AppendLine($"\tBackup name: {databaseReport.BackupName}");
                result.AppendLine($"\tUploaded to {Enum.GetName(typeof(UploadType), report.UploadType)}: {databaseReport.IsBackupUploaded}");
                result.AppendLine($"\tTemp backup file deleted: {databaseReport.IsTempBackupDeleted}");
            }
            if (!string.IsNullOrEmpty(databaseReport.LastError))
                result.AppendLine($"\tLast error: {databaseReport.LastError}");
        }

        return result.ToString();
    }

    private string GetSftpVpsStates(WorkerReport report)
    {
        var result = new StringBuilder();

        if (report.RunResult.Result)
            result.AppendLine($"Initial VPS state: {report.RunResult.InitialVpsState}");
        else
            result.AppendLine($"Can't start VPS: {report.RunResult.LastError}");

        if (Settings.StopVpsOnFinish)
        {
            if (report.StopResult.Result)
                result.AppendLine($"Final VPS state: {report.StopResult.FinalVpsState}");
            else
                result.AppendLine($"Can't stop VPS: {report.StopResult.LastError}");
        }
        return result.ToString();
    }
}