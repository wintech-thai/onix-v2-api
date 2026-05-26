using Its.Onix.Api.Models;

namespace Its.Onix.Api.Services
{
    public class RevenueSummary
    {
        public decimal TotalPayInAmount { get; set; }
        public decimal TotalPayOutAmount { get; set; }
        public decimal TotalPayInFee { get; set; }
        public decimal TotalPayOutFee { get; set; }
        public List<MerchantSummaryData> PayInByMerchant { get; set; }
        public List<MerchantSummaryData> PayOutByMerchant { get; set; }
        public List<DailyRevenueSummaryData> DailyRevenue { get; set; }

        public RevenueSummary()
        {
            PayInByMerchant = [];
            PayOutByMerchant = [];
            DailyRevenue = [];
        }
    }
}
