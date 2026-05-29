using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVPaymentRequest
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MPaymentRequest? PaymentRequest { get; set; }
        public MPaymentTransaction? PayoutTransaction { get; set; }
    }
}
