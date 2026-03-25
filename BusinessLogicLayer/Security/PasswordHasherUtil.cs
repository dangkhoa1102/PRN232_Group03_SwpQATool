using System.Security.Cryptography;
using System.Text;

namespace BusinessLogicLayer.Security;

public static class PasswordHasherUtil
{
    public static bool VerifyPassword(string rawPassword, string storedPassword)
    {
        // Simple comparison without hashing
        return string.Equals(rawPassword, storedPassword, StringComparison.OrdinalIgnoreCase);
    }
}
