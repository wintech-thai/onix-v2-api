using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class MAggregateData
    {
        public string? AggregateKey1 { get; set; }
        public string? AggregateKey2 { get; set; }
        public string? AggregateKey3 { get; set; }

        public int? AggregateCount1 { get; set; }
        public int? AggregateCount2 { get; set; }
        public int? AggregateCount3 { get; set; }

        public decimal? AggregateAmount1 { get; set; }
        public decimal? AggregateAmount2 { get; set; }
        public decimal? AggregateAmount3 { get; set; }

        public MAggregateData()
        {
        }
    }
}
