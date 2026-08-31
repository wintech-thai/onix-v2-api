using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMRiskPolicy : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? Status { get; set; }
    }
}
