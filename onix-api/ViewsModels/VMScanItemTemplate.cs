using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMScanItemTemplate : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
