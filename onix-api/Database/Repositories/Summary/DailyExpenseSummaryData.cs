using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class DailyExpenseSummaryData
    {
        public DateTime? Date { get; set; }
        public decimal? Amount { get; set; }
        public int? Count { get; set; }

        public DailyExpenseSummaryData() { }
    }
}
