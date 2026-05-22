using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class MerchantSummaryData
    {
        public string? MerchantCode { get; set; }
        public string? MerchantStatus { get; set; }

        public int? MerchantCount { get; set; }
        public decimal? TxAmount { get; set; }
        public decimal? FeeAmount { get; set; }
        public decimal? BalanceAmount { get; set; }

        public MerchantSummaryData()
        {
        }
    }
}
