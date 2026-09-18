using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class PayerSummaryData
    {
        public string? PayerName { get; set; }
        public string? MerchantCode { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? FirstSeenDate { get; set; }
        public DateTime? LastSeenDate { get; set; }

        public PayerSummaryData()
        {
        }
    }
}
