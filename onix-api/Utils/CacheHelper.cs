using System.Text.Json;
using StackExchange.Redis;

namespace Its.Onix.Api.Utils
{
    public class CacheHelper
    {
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
