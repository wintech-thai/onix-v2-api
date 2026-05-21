using Its.Onix.Api.Models;

namespace Its.Onix.Api.Services
{
    public class MerchantSummary
    {
        public int MerchantCount { get; set; }
        public List<MAggregateData> MerchantCountByStatus { get; set; }
        public List<MAggregateData> MerchantsBalances { get; set; }
        public List<MAggregateData> MerchantsPayInSummary { get; set; }
        public List<MAggregateData> MerchantsPayOutSummary { get; set; }

        public MerchantSummary()
        {
            MerchantCountByStatus = [];
            MerchantsBalances = [];
            MerchantsPayOutSummary = [];
            MerchantsPayInSummary = [];
        }
    }
}
