using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMScanItemFolder : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
