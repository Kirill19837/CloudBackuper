using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace DbBackuper;

public class ProtectedStreamJsonConfigurationProvider : JsonConfigurationProvider
{
    public ProtectedStreamJsonConfigurationProvider(ProtectedJsonConfigurationSource source) : base(source)
    {
    }

    public override void Load(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();

        var decryptedText = text;
        try
        {
            decryptedText = AesEncryptor.DecryptStringFromBase64_Aes(text);
        }
        catch
        {
            // ignored
        }

        var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream);
        writer.Write(decryptedText);
        writer.Flush();
        memoryStream.Position = 0;
        base.Load(memoryStream);
    }
}

public class ProtectedJsonConfigurationSource : JsonConfigurationSource
{
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new ProtectedStreamJsonConfigurationProvider(this);
    }
}

public static class ProtectedJsonConfigurationExtensions
{
    public static IConfigurationBuilder AddProtectedJsonFile(this IConfigurationBuilder builder, string path, bool optional,
        bool reloadOnChange)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("File path must be a non-empty string.");
        }

        var source = new ProtectedJsonConfigurationSource
        {
            FileProvider = null,
            Path = path,
            Optional = optional,
            ReloadOnChange = reloadOnChange
        };

        source.ResolveFileProvider();
        builder.Add(source);
        return builder;
    }
}
public static class AesEncryptor
{
    public static readonly string CipherKey = "5SK9+X0fVBdtHPLVotaLZA==";
    public static readonly string CipherIV = "ATRo0UuvTCu95JJJEKOF2w==";

    public static string EncryptStringToBase64_Aes(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));

        var key = Convert.FromBase64String(CipherKey);
        var iv = Convert.FromBase64String(CipherIV);


        using var aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.IV = iv;

        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }
        var encrypted = msEncrypt.ToArray();


        return Convert.ToBase64String(encrypted);
    }

    public static string DecryptStringFromBase64_Aes(string base64CipherText)
    {
        if (string.IsNullOrEmpty(base64CipherText))
            throw new ArgumentNullException(nameof(base64CipherText));

        var cipherText = Convert.FromBase64String(base64CipherText);
        var key = Convert.FromBase64String(CipherKey);
        var iv = Convert.FromBase64String(CipherIV);

        using var aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.IV = iv;

        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using var msDecrypt = new MemoryStream(cipherText);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        var plaintext = srDecrypt.ReadToEnd();

        return plaintext;
    }
}