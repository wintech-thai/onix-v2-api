using Microsoft.AspNetCore.Identity;

namespace Its.Onix.Api.Utils
{
    //one-way hash (PBKDF2 ผ่าน ASP.NET Identity) ตัวเดียวกับที่ใช้ hash password login เข้าโปรแกรมอยู่แล้ว (AuthServiceNative)
    public static class PasswordHashUtils
    {
        private static readonly PasswordHasher<object> hasher = new();

        public static string HashPassword(string password)
        {
            return hasher.HashPassword(null!, password);
        }

        public static bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            return hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword) != PasswordVerificationResult.Failed;
        }
    }
}
