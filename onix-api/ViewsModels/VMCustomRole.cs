using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMCustomRole : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? Level { get; set; }
    }
}
