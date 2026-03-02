using System;
using System.Security.Cryptography;
using System.Text;

namespace App.Core.Security;

public static class ConnectionSecurity
{
    #region Methods

    public static string Encrypt(string plainText)
    {
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.LocalMachine);

        return Convert.ToBase64String(encrypted);
    }

    public static string Decrypt(string cipherText)
    {
        var bytes = Convert.FromBase64String(cipherText);
        var decrypted = ProtectedData.Unprotect(bytes, null, DataProtectionScope.LocalMachine);

        return Encoding.UTF8.GetString(decrypted);
    }

    #endregion
}