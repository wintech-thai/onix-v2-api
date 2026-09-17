using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMBankSummary : VMQueryBase
    {
        public string? BankCode { get; set; }
        public string? AccountNumber { get; set; }
        public string? MerchantCode { get; set; }
        public bool IncludeP2P { get; set; }

        public VMBankSummary()
        {
            IncludeP2P = false;
        }
    }
}
