using System;
using System.Security.Cryptography;
using System.Text;

namespace AppleStore.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] combined = new byte[salt.Length + passwordBytes.Length];
            Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
            Buffer.BlockCopy(passwordBytes, 0, combined, salt.Length, passwordBytes.Length);

            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(combined);
                byte[] result = new byte[salt.Length + hash.Length];
                Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
                Buffer.BlockCopy(hash, 0, result, salt.Length, hash.Length);

                return Convert.ToBase64String(result);
            }
        }
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            byte[] fullBytes = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[16];
            Buffer.BlockCopy(fullBytes, 0, salt, 0, salt.Length);
            byte[] storedHash = new byte[fullBytes.Length - salt.Length];
            Buffer.BlockCopy(fullBytes, salt.Length, storedHash, 0, storedHash.Length);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] combined = new byte[salt.Length + passwordBytes.Length];
            Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
            Buffer.BlockCopy(passwordBytes, 0, combined, salt.Length, passwordBytes.Length);

            using (var sha256 = SHA256.Create())
            {
                byte[] computedHash = sha256.ComputeHash(combined);
                if (computedHash.Length != storedHash.Length)
                    return false;

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i])
                        return false;
                }
                return true;
            }
        }
    }
}