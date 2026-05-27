using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPaymentRequest : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? Direction { get; set; }
        public string? Status { get; set; }
        public string? BankAccountId { get; set; }
        public string? GeneratedAmountStr { get; set; }
        public string? MerchantId { get; set; }
    }
}
