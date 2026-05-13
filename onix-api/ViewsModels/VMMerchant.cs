using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMMerchant : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? Status { get; set; }
    }
}
