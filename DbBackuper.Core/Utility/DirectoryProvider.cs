using Amazon.S3;
using DbBackuper.Core.Models;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbBackuper.Core.Utility
{
    public static class DirectoryProvider
    {
        public static IEnumerable<(string ArchiveName, string ArchivePath, string DirectoryName)> CreateDirectoryArchives(WorkerSettings settings)
        {
            foreach(var folderToBackup in settings.FoldersPaths)
            {
                var folderName = folderToBackup.Split('\\').LastOrDefault() ?? folderToBackup;
                var fileName = $"{folderName}{DateTime.UtcNow:yyyy-dd-M-HH-mm-ss}.zip";
                var destinationArchiveName = Path.Combine(settings.BackupsTempFolder, fileName);
                ZipFile.CreateFromDirectory(folderToBackup, destinationArchiveName, CompressionLevel.Optimal, true);
                yield return (fileName, destinationArchiveName, folderName);
            }
        } 
    }
}
