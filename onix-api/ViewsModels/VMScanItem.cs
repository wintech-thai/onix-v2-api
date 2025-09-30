using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMScanItem : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
