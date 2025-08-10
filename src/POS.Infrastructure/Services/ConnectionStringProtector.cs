using System.Security.Cryptography;
using System.Text;
using POS.Shared.Infrastructure;

namespace POS.Infrastructure.Services;

public class ConnectionStringProtector : IConnectionStringProtector
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public ConnectionStringProtector()
    {
        // Demo only: derive fixed key/iv. Replace with data protection provider / key vault in production.
        _key = SHA256.HashData(Encoding.UTF8.GetBytes("zipos-demo-key"));
        _iv = MD5.HashData(Encoding.UTF8.GetBytes("zipos-demo-iv"));
    }

    public string Protect(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return Convert.ToBase64String(cipherBytes);
    }

    public string Unprotect(string protectedValue)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var cipherBytes = Convert.FromBase64String(protectedValue);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}

