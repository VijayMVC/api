using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Cllearworks.COH.Utility
{
    public static class PasswordHelpers
    {
        public static string GenerateHashForSaltAndPassword(string base64Salt, string password)
        {
            byte[] bytes = UTF8Encoding.UTF8.GetBytes(password);
            byte[] saltBytes = Convert.FromBase64String(base64Salt);

            byte[] allBytes = new byte[bytes.Length + saltBytes.Length];
            bytes.CopyTo(allBytes, 0);
            saltBytes.CopyTo(allBytes, bytes.Length);
            SHA256Managed hash = new SHA256Managed();

            return Convert.ToBase64String(hash.ComputeHash(allBytes));
        }

        public static void GenerateSaltAndHash(string password, out string salt, out string passwordHash)
        {
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] saltB = new byte[32]; //256 bits
            random.GetBytes(saltB);
            salt = Convert.ToBase64String(saltB);

            passwordHash = GenerateHashForSaltAndPassword(salt, password);
        }

        public static bool IsValidPassword(string password, PasswordRequirements reqs = null)
        {
            if (reqs == null)
            {
                if (password.Length < 8)
                    return false;
                if (!Regex.IsMatch(password, "^(?=.*[a-z])"))
                    return false;
                if (!Regex.IsMatch(password, "^(?=.*[A-Z])"))
                    return false;
                if (!Regex.IsMatch(password, "^(?=.*[0-9])"))
                    return false;
                if (!Regex.IsMatch(password, "^(?=.*[^a-zA-Z0-9])"))
                    return false;

                return true;
            }
            else
            {
                return reqs.Validate(password);
            }
        }
    }
}
