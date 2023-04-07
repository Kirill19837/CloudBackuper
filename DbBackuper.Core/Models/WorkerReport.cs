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
}

public class DatabaseReport : Report
{
    public DatabaseSetting? DatabaseSetting { get; set; }
}

public class DirectoryReport : Report
{
    public string DirectoryName { get; set; }
}

public class WorkerReport
{
    public List<Report> Reports { get; set; }
    public UploadType UploadType { get; set; }
    public (bool Result, string InitialVpsState, string? LastError) RunResult { get; set; }
    public (bool Result, string FinalVpsState, string? LastError) StopResult { get; set; }
}
