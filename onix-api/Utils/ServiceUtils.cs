using System.Text;
using System.Text.RegularExpressions;

namespace Its.Onix.Api.Utils
{
    public static class ServiceUtils
    {
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

        public static string GetOrgId(HttpRequest request)
        {
            var pattern = @"^\/api\/(.+)\/org\/(.+)\/action\/(.+)$";
            var path = request.Path;
            MatchCollection matches = Regex.Matches(path, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));

            var orgId = matches[0].Groups[2].Value;

            return orgId;
        }
    }
}
