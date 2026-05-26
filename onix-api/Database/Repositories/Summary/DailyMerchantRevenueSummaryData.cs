using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class DailyMerchantRevenueSummaryData
    {
        public DateTime? Date { get; set; }
        public string? MerchantCode { get; set; }
        public decimal? PayInAmount { get; set; }
        public decimal? PayOutAmount { get; set; }
        public decimal? PayInFee { get; set; }
        public decimal? PayOutFee { get; set; }

        public DailyMerchantRevenueSummaryData()
        {
        }
    }
}
