using System.Security.Cryptography;
using System.Text;

namespace SWP_Q_A_Tools_APIs.Common.Security;

public static class PasswordHasherUtil
{
    public static string HashPassword(string rawPassword)
    {
        var bytes = Encoding.UTF8.GetBytes(rawPassword);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    public static bool VerifyPassword(string rawPassword, string storedHash)
    {
        var incomingHash = HashPassword(rawPassword);
        return string.Equals(incomingHash, storedHash, StringComparison.OrdinalIgnoreCase);
    }
}
