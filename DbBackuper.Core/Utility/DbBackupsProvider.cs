using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.IO;

namespace DbBackuper.Core.Utility;

public static class DbBackupsProvider
{
    public static FileInfo BackupDatabase(string databaseName, string userName, string password, string serverName, string destinationPath)
    {
        var now = DateTime.Now;
        var sqlBackup = new Backup
        {
            Action = BackupActionType.Database,
            BackupSetDescription = "BackUp of:" + databaseName + "on" + now.ToShortDateString(),
            BackupSetName = "FullBackUp",
            Database = databaseName,
            Initialize = true,
            Checksum = true,
            ContinueAfterError = true,
            Incremental = false,
            //ExpirationDate = now.AddDays(3),//TODO: settings
            //LogTruncation = BackupTruncateLogType.NoTruncate,
            FormatMedia = false,
            CompressionOption = BackupCompressionOptions.On
        };

        string backupFileName = $"{databaseName}-{now:yyyy-dd-M-HH-mm-ss}-FullBackUp.bak";
      
        CheckIfFolder(destinationPath);

        string backupFilePath = Path.Combine(destinationPath, backupFileName);
        var deviceItem = new BackupDeviceItem(backupFilePath, DeviceType.File);
        var connection = new ServerConnection(serverName, userName, password); 
        var sqlServer = new Server(connection)
        {
            ConnectionContext = {
                StatementTimeout = 60 * 60
            }
        };

        sqlBackup.Devices.Add(deviceItem);
        sqlBackup.SqlBackup(sqlServer);
        sqlBackup.Devices.Remove(deviceItem);

        return new FileInfo(backupFilePath);
    }

    private static void CheckIfFolder(string folderPath)
    {
        bool exists = System.IO.Directory.Exists(folderPath);

        if (!exists)
            System.IO.Directory.CreateDirectory(folderPath);
    }
}