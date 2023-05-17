using System.Text;
using DbBackuper.Core.Extensions;

namespace DbBackuper.Core.Models;

public enum UploadType
{
    AWS,
    SFTP
}

public class Report
{
    public string BackupName { get; set; }
    public bool IsBackupCreated { get; set; }
    public bool IsBackupUploaded { get; set; }
    public bool IsTempBackupDeleted { get; set; }
    public string BackupTempLocation { get; set; }
    public string? LastError { get; set; }
    public long BackupSize { get; set; }

    public virtual string GetReportRaw(WorkerReport workerReport, WorkerSettings workerSettings)
    {
        var result = new StringBuilder();

        result.AppendLine($"\tBackup name: {BackupName}");
        result.AppendLine($"\tBackup size: {BackupSize.FormatBytes()}");
        result.AppendLine($"\tUploaded to {Enum.GetName(typeof(UploadType), workerReport.UploadType)}: {IsBackupUploaded}");
        result.AppendLine($"\tTemp backup file deleted: {IsTempBackupDeleted}");

        if (!string.IsNullOrEmpty(LastError))
            result.AppendLine($"\tLast error: {LastError}");

        return result.ToString();
    }

    protected string GetSftpVpsStates(WorkerReport report, WorkerSettings workerSettings)
    {
        var result = new StringBuilder();

        if (report.RunResult.Result)
            result.AppendLine($"Initial VPS state: {report.RunResult.InitialVpsState}");
        else
            result.AppendLine($"Can't start VPS: {report.RunResult.LastError}");

        if (workerSettings.StopVpsOnFinish)
        {
            if (report.StopResult.Result)
                result.AppendLine($"Final VPS state: {report.StopResult.FinalVpsState}");
            else
                result.AppendLine($"Can't stop VPS: {report.StopResult.LastError}");
        }
        return result.ToString();
    }
}

public class DatabaseReport : Report
{
    public DatabaseSetting? DatabaseSetting { get; set; }
    public override string GetReportRaw(WorkerReport workerReport, WorkerSettings workerSettings)
    {
        var result = new StringBuilder();
        if (workerReport.UploadType == UploadType.SFTP)
            result.AppendLine(GetSftpVpsStates(workerReport, workerSettings));
        if (DatabaseSetting != null)
        {
            result.AppendLine($"Database [{DatabaseSetting.DatabaseName}]:");
        }
        result.AppendLine(base.GetReportRaw(workerReport, workerSettings));

        return result.ToString();
    }
}

public class ArchiveResult
{
    public ArchiveResult(string archiveName, string archivePath, string directoryName, IEnumerable<FileArchiveError> archiveErrors)
    {
        ArchiveName = archiveName;
        ArchiveInfo = new FileInfo(archivePath);
        DirectoryName = directoryName;
        ArchiveErrors = archiveErrors;
    }

    public string ArchiveName { get; init; }
    public FileInfo ArchiveInfo { get; init; }
    public string DirectoryName { get; init; }
    public IEnumerable<FileArchiveError> ArchiveErrors { get; init; } 
}

public class FileArchiveError
{
    public FileArchiveError(string filePath, string errorMessage)
    {
        FilePath = filePath;
        ErrorMessage = errorMessage;
    }

    public string FilePath { get; set; }
    public string ErrorMessage { get; set; }

    public string GetErrorMessage()
    {
        return $"File path: {FilePath} \n \t\t\tError message: {ErrorMessage}";
    }
}

public class DirectoryReport : Report
{
    public string DirectoryName { get; set; }
    public IEnumerable<FileArchiveError> FileArchiveErrors { get; set; } = new List<FileArchiveError>();

    public override string GetReportRaw(WorkerReport workerReport, WorkerSettings workerSettings)
    {
        var result = new StringBuilder();
        result.AppendLine($"Directory [{DirectoryName}]: ");
        result.AppendLine(base.GetReportRaw(workerReport, workerSettings));

        if(!FileArchiveErrors.Any())
            return result.ToString();

        result.AppendLine("\tFile archive errors: ");
        foreach(var error in FileArchiveErrors)
        {
            result.AppendLine("\t\t" + error.GetErrorMessage());
        }

        return result.ToString();
    }
}

public class WorkerReport
{
    public List<Report> Reports { get; set; }
    public UploadType UploadType { get; set; }
    public (bool Result, string InitialVpsState, string? LastError) RunResult { get; set; }
    public (bool Result, string FinalVpsState, string? LastError) StopResult { get; set; }
}
