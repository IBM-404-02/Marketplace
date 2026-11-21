using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MarketplaceSWAGAwpf
{
    internal class Core
    {
        public static MarketplaceDbEntities Context = new MarketplaceDbEntities();

        public static Users CurrentUser = null;

        public static bool IsAuthorized => CurrentUser != null;

        public static bool IsAdmin => CurrentUser != null && CurrentUser.RoleId == 2;

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
