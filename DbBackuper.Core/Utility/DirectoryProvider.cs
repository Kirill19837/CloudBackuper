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
        private const CompressionLevel _compressionLevel = CompressionLevel.SmallestSize;
        public static ArchiveResult CreateDirectoryArchive(WorkerSettings settings, string folderToBackup)
        {
            Console.WriteLine($"Creating archive for {folderToBackup} ...");
            var folderName = folderToBackup.Split('\\').LastOrDefault() ?? folderToBackup;
            var fileName = $"{folderName}{DateTime.UtcNow:yyyy-dd-M-HH-mm-ss}.zip";
            var destinationArchiveName = Path.Combine(settings.BackupsTempFolder, fileName);

            using ZipArchive archive = ZipFile.Open(destinationArchiveName, ZipArchiveMode.Create);
            var archiveErrors = new List<FileArchiveError>();
            AddFolderToZip(folderToBackup, archive, folderToBackup.Length + 1, "", ref archiveErrors);

            return new ArchiveResult(fileName, destinationArchiveName, folderName, archiveErrors);
        }

        private static void AddFolderToZip(string folderPath, ZipArchive archive, int rootFolderLength, string folderName, ref List<FileArchiveError> archiveErrors)
        {
            string[] fileEntries = Directory.GetFiles(folderPath);
            foreach (string filePath in fileEntries)
            {
                try
                {
                    string entryName = string.Concat(folderName, "/", filePath.AsSpan(rootFolderLength)).TrimStart('/');
                    using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    ZipArchiveEntry entry = archive.CreateEntry(entryName, _compressionLevel);
                    using Stream entryStream = entry.Open();
                    fileStream.CopyTo(entryStream);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Can't archive file {filePath}. Reason: {ex.Message}");
                    archiveErrors.Add(new FileArchiveError(filePath, ex.Message));
                }
            }

            string[] subfolders = Directory.GetDirectories(folderPath);
            foreach (string subfolder in subfolders)
            {
                string folderNameWithPath = folderName + "/" + Path.GetFileName(subfolder);
                archive.CreateEntry(folderNameWithPath + "/");
                AddFolderToZip(subfolder, archive, rootFolderLength, folderNameWithPath, ref archiveErrors);
            }
        }
    }
}
