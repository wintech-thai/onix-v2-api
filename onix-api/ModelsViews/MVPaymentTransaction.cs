using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVPaymentTransaction
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MPaymentTransaction? PaymentTransaction { get; set; }
    }
}
