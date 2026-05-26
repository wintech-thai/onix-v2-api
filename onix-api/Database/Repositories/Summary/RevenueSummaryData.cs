using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class RevenueSummaryData
    {
        public string? Direction { get; set; }
        public decimal? TxAmount { get; set; }
        public decimal? FeeAmount { get; set; }

        public RevenueSummaryData()
        {
        }
    }
}
