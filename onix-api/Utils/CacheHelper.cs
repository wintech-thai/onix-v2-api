
namespace Its.Onix.Api.Utils
{
    public class CacheHelper
    {
        public static string CreateApiOtpKey(string orgId, string apiName)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";

            var key = $"{orgId}:{environment}:{apiName}";
            return key;
        }

        public static string CreateLoginSessionKey(string userName)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";

            var key = $"LoginSession:{environment}:{userName}";
            return key;
        }

        public static string CreateAdminLoginSessionKey(string userName)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";

            var key = $"AdminLoginSession:{environment}:{userName}";
            return key;
        }

        public static string CreateAuditLogStreamKey()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";

            var key = $"AuditLog:{environment}";
            return key;
        }

        public static string CreateScanItemActionCacheLoaderKey(string orgId)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";
            var key = $"CacheLoader:{environment}:ScanItemActions:{orgId}";
            return key;
        }

        public static string CreatePointTriggerCustRegisterKey(string orgId)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";
            var key = $"PointTrigger:{environment}:CustRegisterPointTrigger:{orgId}";
            return key;
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
