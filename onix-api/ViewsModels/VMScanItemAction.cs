using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMScanItemAction : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
