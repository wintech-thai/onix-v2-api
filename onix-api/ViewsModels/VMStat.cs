using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMStat : VMQueryBase
    {
        public string? StatCode { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
    }
}
