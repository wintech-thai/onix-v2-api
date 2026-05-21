using Its.Onix.Api.Models;

namespace Its.Onix.Api.Services
{
    public class MerchantSummary
    {
        public int MerchantCount { get; set; }
        public List<MerchantSummaryData> MerchantCountByStatus { get; set; }
        public List<MerchantSummaryData> MerchantsBalances { get; set; }
        public List<MerchantSummaryData> MerchantsPayInSummary { get; set; }
        public List<MerchantSummaryData> MerchantsPayOutSummary { get; set; }

        public MerchantSummary()
        {
            MerchantCountByStatus = [];
            MerchantsBalances = [];
            MerchantsPayOutSummary = [];
            MerchantsPayInSummary = [];
        }
    }
}
