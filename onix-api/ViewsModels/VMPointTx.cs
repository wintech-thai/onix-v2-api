using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMPointTx : VMQueryBase
    {
        public string? WalletId { get; set; }
    }
}
