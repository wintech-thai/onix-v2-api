using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class DailyRevenueSummaryData
    {
        public DateTime? Date { get; set; }
        public decimal? PayInFee { get; set; }
        public decimal? PayOutFee { get; set; }

        public DailyRevenueSummaryData()
        {
        }
    }
}
