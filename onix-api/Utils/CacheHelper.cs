using System.Text.Json;
using StackExchange.Redis;

namespace Its.Onix.Api.Utils
{
    public class CacheHelper
    {
        private readonly static string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        public static string CreateApiOtpKey(string orgId, string apiName)
        {
            return $"{orgId}:{environment}:{apiName}";
        }

        public static string CreateScanItemActionKey(string orgId)
        {
            //TODO : Use environment as key component
            return $"{orgId}:ScanItemAction";
        }

        public static string CreateScanItemTemplateKey(string orgId)
        {
            //TODO : Use environment as key component
            return $"{orgId}:ScanItemTemplate";
        }
    }
}
