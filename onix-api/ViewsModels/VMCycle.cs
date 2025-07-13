using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMCycle : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public int? CycleType { get; set; }
        public DateTime? StargDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
