using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMBankAccount : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? AccountCategory { get; set; }
        public string? AccountLevel { get; set; }
        public string? AccountType { get; set; }
    }
}
