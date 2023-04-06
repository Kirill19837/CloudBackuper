# DbBackuper
Tool to backup data to SFTP
1.	Copy your json settings to https://demo.kenhaggerty.com/demos/aescipher. (Plain Text field)
2.	Enter your Base64 Key (5SK9+X0fVBdtHPLVotaLZA==) and Base64 IV (ATRo0UuvTCu95JJJEKOF2w==)
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
