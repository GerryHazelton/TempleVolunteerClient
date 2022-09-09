using System.Security.Cryptography;
using System.Text;

namespace SRFSDP.API.Common
{
    public class SecurityHelper
    {
        public static string GenerateSalt(int nSalt)
        {
            var saltBytes = new byte[nSalt];

            using (var provider = new RSACryptoServiceProvider())
            {
                provider.EncryptValue(saltBytes);
            }

            return Convert.ToBase64String(saltBytes);
        }

        public static string HashPassword(string password, string salt, int nIterations, int nHash)
        {
            var saltBytes = Convert.FromBase64String(salt);

            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltBytes, nIterations))
            {
                return Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(nHash));
            }
        }

        public static string GetPasswordHash(string password, string userId)
        {
            using (var md5 = MD5.Create())
            {
                byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
                byte[] hash = md5.ComputeHash(passwordBytes);
                var sbHash = new StringBuilder();

                for (int i = 0; i < hash.Length; i++)
                {
                    sbHash.Append(hash[i].ToString("X2"));
                }

                return sbHash.ToString();
            }
        }
    }
}
