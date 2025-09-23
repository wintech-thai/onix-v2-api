using System.Text;
using System.Text.RegularExpressions;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Utils
{
    public static class ServiceUtils
    {
        private static readonly string[] whiteListedApi = [
                "Organization:GetUserAllowedOrg",
                "User:UpdatePassword"
            ];

        public static bool IsWhiteListedAPI(string controller, string api)
        {
            //จะไม่ต้อง verify user แต่ยังต้อง validate JWT token อยู่
            var whiteListedKey = $"{controller}:{api}";

            return whiteListedApi.Contains(whiteListedKey);
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
    }
}
