using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMCycle : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public int? CycleType { get; set; }
        public DateTime? FromStargDate { get; set; }
        public DateTime? ToStargDate { get; set; }
        public DateTime? FromEndDate { get; set; }
        public DateTime? ToEndDate { get; set; }
    }
}
