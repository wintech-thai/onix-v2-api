using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPayerSummary : VMQueryBase
    {
        public string? MerchantCode { get; set; }
        public string? PayerName { get; set; }
    }
}
