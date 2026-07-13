using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class ExpenseByCategoryData
    {
        public string? Code { get; set; }
        public string? Desc { get; set; }
        public decimal? Amount { get; set; }
        public int? Count { get; set; }

        public ExpenseByCategoryData() { }
    }
}
