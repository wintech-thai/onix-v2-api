using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Utils
{
    public static class ServiceUtils
    {
        private static readonly Random _random = new Random();
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private static readonly string[] whiteListedApi = [
                "OnlyUser:GetUserAllowedOrg",
                "OnlyUser:UpdatePassword",
                "OnlyUser:Logout"
            ];

        public static bool IsWhiteListedAPI(string controller, string api)
        {
            //จะไม่ต้อง verify user แต่ยังต้อง validate JWT token อยู่
            var whiteListedKey = $"{controller}:{api}";

            return whiteListedApi.Contains(whiteListedKey);
        }

        public static string MaskScanItemPin(string pin)
        {
            if (string.IsNullOrEmpty(pin))
                return pin;

            if (string.IsNullOrEmpty(pin) || pin.Length <= 2)
                return pin;

            return pin[0] + new string('*', pin.Length - 2) + pin[^1];
        }

        public static string GenerateSecureRandomString(int length)
        {
            if (length <= 0) throw new ArgumentException("Length must be greater than zero.");

            char[] result = new char[length];
            byte[] randomBytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            for (int i = 0; i < length; i++)
            {
                result[i] = _chars[randomBytes[i] % _chars.Length];
            }

            return new string(result);
        }

        public static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            var parts = email.Split('@');
            if (parts.Length != 2)
                return email; // ถ้าไม่ใช่อีเมลที่ถูกต้อง คืนค่าเดิมไป

            var local = parts[0];
            var domain = parts[1];

            if (local.Length == 1)
            {
                // ถ้ามีแค่ตัวเดียว ก็ mask เป็น *
                return $"* @{domain}";
            }
            else if (local.Length == 2)
            {
                // ถ้ามีสองตัว ก็โชว์ทั้งสองตัวเลย
                return $"{local}@{domain}";
            }
            else
            {
                // เก็บตัวแรกและตัวสุดท้าย
                var maskedLocal = local[0]
                                + new string('*', local.Length - 2)
                                + local[^1];
                return $"{maskedLocal}@{domain}";
            }
        }

        public static MVEntityRestrictedInfo MaskingEntity(MVEntity entity)
        {
            var m = new MVEntityRestrictedInfo()
            {
                Status = entity.Status,
                Description = entity.Description,
            };

            if (entity.Entity != null)
            {
                m.MaskingEmail = MaskEmail(entity.Entity!.PrimaryEmail!);
            }

            return m;
        }

        public static string CreateOTP(int length)
        {
            var random = new Random();
            var sb = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                sb.Append(random.Next(0, 10)); // สุ่มตัวเลข 0-9
            }

            return sb.ToString();
        }

        public static bool IsGuidValid(string guid)
        {
            try
            {
                Guid.Parse(guid);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int ReadInt32BigEndian(byte[] bytes, int offset)
        {
            return (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
        }

        public static PathComponent GetPathComponent(HttpRequest request)
        {
            var pattern = @"^\/api\/(.+)\/org\/(.+)\/action\/(.+)$";
            var path = request.Path;
            MatchCollection matches = Regex.Matches(path, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));

            var result = new PathComponent()
            {
                OrgId = matches[0].Groups[2].Value,
                ControllerName = matches[0].Groups[1].Value,
                ApiName = matches[0].Groups[3].Value,
            };

            return result;
        }
        
        public static string? GetValueFromTags(string key, string tags)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(tags))
                return null;

            var parts = tags.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var kv = part.Split('=', 2); // split ครั้งเดียว เผื่อค่ามี '=' ข้างใน
                if (kv.Length == 2 && kv[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return kv[1].Trim();
                }
            }

            return null;
        }
    }
}
