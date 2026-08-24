using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMCurrencyAccount : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencyCategory { get; set; }

        public string? BankCode { get; set; }
        public string? AccountType { get; set; }
        public string? AccountLevel { get; set; }
    }
}
