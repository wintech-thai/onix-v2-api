using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPaymentGateway : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? Scheme { get; set; }
    }
}
