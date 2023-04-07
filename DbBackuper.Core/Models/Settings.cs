namespace DbBackuper.Core.Models
{
    public sealed class WorkerSettings
    {
        public int? BackupDays { get; set; }
        public string BackupMask { get; set; }
        public int? NumberOfAttemptsToStartSftp { get; set; }
        public int? NumberOfAttemptsToStopSftp { get; set; }
        public string ApiKey { get; set; }
        public string ApiRoot { get; set; }

        public string VpsIp { get; set; }
        public string VpsId { get; set; }

        public string SftpUserName { get; set; }
        public string SftpUserPassword { get; set; }

        public int WaitVpsStatusRetryDelaySec { get; set; }
        public int WaitVpsStatusRetryCount { get; set; }

        public string BackupsTempFolder { get; set; }
        public bool StopVpsOnFinish { get; set; }
        public bool UseAWS { get; set; }

        public MailSettings MailSettings { get; set; }

        public List<DatabaseSetting> DatabaseSettings { get; set; }
        public List<string> FoldersPaths { get; set; }
        public AWSSettings AWSSettings { get; set; }
    }

    public sealed class MailSettings
    {
        public bool SendEmailReport { get; set; }
        public string MailFrom { get; set; }
        public string DisplayNameFrom { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string MailTo { get; set; }
        public string Subject { get; set; }
    }

    public sealed class DatabaseSetting
    {
        public string DatabaseName { get; set; }
        public string ServerName { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
    }
    public sealed class AWSSettings
    {
        public string BucketName { get; set; }
        public string SecretKey { get; set; }
        public string AccessKey { get; set; }
    }

}
