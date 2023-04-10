using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using DbBackuper.Core.Models;
using Microsoft.Extensions.Options;

namespace DbBackuper.Core.AWS
{
    public class AWSUploadResult
    {
        public AWSUploadResult() { }
        public AWSUploadResult(PutObjectResponse response)
        {
            Response = response;
            IsSuccess = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            LastErrorMessage = string.Empty;
        }
        public PutObjectResponse? Response { get; set; }
        public bool IsSuccess { get; set; }
        public string? LastErrorMessage { get; set; }
    }
    public class AWSUploader
    {
        private static readonly RegionEndpoint _bucketRegion = RegionEndpoint.EUNorth1;
        private readonly IAmazonS3 _client;
        private readonly WorkerSettings Settings;

        public AWSUploader(IOptions<WorkerSettings> workerSettings)
        {
            Settings = workerSettings.Value;
            var credentials = new BasicAWSCredentials(Settings.AWSSettings.AccessKey, Settings.AWSSettings.SecretKey);
            _client = new AmazonS3Client(credentials, _bucketRegion);
        }

        public async Task<Report> UploadToAWS(Report report)
        {
            var folderName = await GetFolderName(report);
            var uploadResult = await UploadToAWS(report.BackupTempLocation, Path.Combine(folderName, report.BackupName));
            if(uploadResult.IsSuccess)
            {
                report.IsBackupUploaded = true;
                return report;
            }

            report.IsBackupUploaded = false;
            report.LastError = uploadResult.LastErrorMessage;
            return report;
        }

        public async Task<AWSUploadResult> UploadToAWS(string filePath, string objectKey)
        {
            Console.WriteLine($"Uploading to aws {filePath} ...");
            var uploadResult = new AWSUploadResult();
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = Settings.AWSSettings.BucketName,
                    Key = objectKey,
                    FilePath = filePath,
                };

                uploadResult.Response = await _client.PutObjectAsync(putRequest);
                uploadResult.IsSuccess = true;
            }
            catch (AmazonS3Exception e)
            {
                var errorMessage = "[AWS] Upload error. Message: {0}" + e.Message;
                Console.WriteLine(errorMessage);
                uploadResult.IsSuccess = false;
                uploadResult.LastErrorMessage = errorMessage;
            }
            catch (Exception e)
            {
                var errorMessage = "[APP] Upload to AWS error. Message: {0}" + e.Message;
                Console.WriteLine(errorMessage);
                uploadResult.IsSuccess = false;
                uploadResult.LastErrorMessage = errorMessage;
            }

            return uploadResult;
        }

        private async Task<string> GetFolderName(Report report)
        {
            string? folderName;
            if (report is DatabaseReport databaseReport
                && databaseReport.DatabaseSetting != null)
            {
                folderName = databaseReport.DatabaseSetting.DatabaseName;
            }
            else if (report is DirectoryReport directoryReport)
            {
                folderName = directoryReport.DirectoryName;
            }
            else
            {
                folderName = string.Empty;
            }

            folderName = folderName.TrimEnd('/') + '/';

            if (!string.IsNullOrEmpty(folderName))
                await CreateFolderAsync(folderName);

            return folderName;
        }

        private async Task CreateFolderAsync(string path)
        {
            path = path.TrimEnd('/') + '/';

            var findFolderRequest = new ListObjectsV2Request
            {
                BucketName = Settings.AWSSettings.BucketName,
                Prefix = path,
                MaxKeys = 1
            };
            try
            {
                ListObjectsV2Response findFolderResponse = await _client.ListObjectsV2Async(findFolderRequest);

                if (findFolderResponse.S3Objects.Any())
                {
                    return;
                }

                var request = new PutObjectRequest()
                {
                    BucketName = Settings.AWSSettings.BucketName,
                    Key = path,
                    ContentBody = string.Empty
                };
                PutObjectResponse response = await _client.PutObjectAsync(request);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("[AWS] Folder creation error. Message: {0}", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("[APP] Create AWS folder error. Message: {0}", e.Message);
            }
        }

        private void WriteProfile(string profileName, string keyId, string secret)
        {
            var options = new CredentialProfileOptions
            {
                AccessKey = keyId,
                SecretKey = secret
            };
            var profile = new CredentialProfile(profileName, options);
            var sharedFile = new SharedCredentialsFile();
            sharedFile.RegisterProfile(profile);
        }
    }
}
