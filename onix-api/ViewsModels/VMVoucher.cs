using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMVoucher : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
