# DbBackuper
Tool to backup data to SFTP
1.	Copy your json settings to https://demo.kenhaggerty.com/demos/aescipher. (Plain Text field)
2.	Enter your Base64 Key (5SK9+X0fVBdtHPLVataLZA==) and Base64 IV (ATRo0UuvTCe95JJJEKOF2w==)
3.	Click “encrypt”
4.	Copy encrypted data to your appsettins.json

•	see sample plain appsettins.json below:

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "System": "Warning",
      "Microsoft": "Warning"
    },
    "Console": {
      "IncludeScopes": true,
      "LogLevel": {
        "Default": "Information"
      }
    }
  },
  "Settings": {
    "UseAWS": true,
    "NumberOfAttemptsToStartSftp": 10,
    "NumberOfAttemptsToStopSftp": 10,
    "BackupDays": "14",
    "BackupMask": "*.bak",
    "ApiKey": "************",
    "ApiRoot": "https://some.api",
    "VpsIp": "**.**.**.***",
    "VpsId": "11745808",
    "SftpUserName": "***username***",
    "SftpUserPassword": "******",
    "WaitVpsStatusRetryDelaySec": 10,
    "WaitVpsStatusRetryCount": 10,
    "BackupsTempFolder": "C:\\Database backup",
    "StopVpsOnFinish": true,
    "ProtectPasswords": true,
    "MailSettings": {
      "SendEmailReport": true,
      "MailFrom": "***mail@mail.com***",
      "DisplayNameFrom": "DB backup",
      "User": "***mail@mail.com***",
      "Password": "*******",
      "Host": "***host***",
      "Port": 587,
      "MailTo": "***mailto@mail.com***",
      "Subject": "Database back-up report"
    },
    "DatabaseSettings": [
      {
        "DatabaseName": "***DBNAME***",
        "ServerName": "****",
        "UserName": "****",
        "UserPassword": "********"
      }
    ],
    "AWSSettings": {
      "BucketName": "****bucketname****",
      "AccessKey": "*****",
      "SecretKey": "******"
    }
  }
}
```

# AWS Setup
1. Create a new user through Identity and Access Management (IAM) (or use an existing user).
2. Add this user the rights to create/manage s3 buckets
3. Generate and obtain access keys for this user
4. Use the terraform script in the folder terraform-scripts/bucketsetup.tf to create a new buckets (see readme)
5. After creating a new bucket, insert the user access keys and the name of the bucket in appsettings.json (AWSSettings)
