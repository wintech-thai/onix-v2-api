using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPointBalance : VMQueryBase
    {
        public string? WalletId { get; set; }
        public string? BalanceType { get; set; } //StatCode
        public string? DateKey { get; set; }

    }
}
