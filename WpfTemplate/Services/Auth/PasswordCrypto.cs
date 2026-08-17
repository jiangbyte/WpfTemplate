using System.Security.Cryptography;
using System.Text;

namespace WpfTemplate.Services.Auth;

public static class PasswordCrypto
{
    public static string EncryptRsaOaepSha256(string publicKeyB64OrPem, string plainText)
    {
        var spki = Convert.FromBase64String(NormalizePublicKey(publicKeyB64OrPem));
        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(spki, out _);
        var encrypted = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(encrypted);
    }

    private static string NormalizePublicKey(string publicKeyB64OrPem) =>
        publicKeyB64OrPem
            .Replace("-----BEGIN PUBLIC KEY-----", string.Empty, StringComparison.Ordinal)
            .Replace("-----END PUBLIC KEY-----", string.Empty, StringComparison.Ordinal)
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Trim();
}
