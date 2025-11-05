using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMItemBalance : VMQueryBase
    {
        public string? ItemId { get; set; }
        public string? BalanceType { get; set; } //StatCode
        //public string? DateKey { get; set; }

    }
}
