using System.Security.Cryptography;
using System.Text;

namespace CIMC.Helper;

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public static bool Verify(string password, string hash)
    {
        return string.Equals(HashPassword(password), hash, StringComparison.Ordinal);
    }
}
