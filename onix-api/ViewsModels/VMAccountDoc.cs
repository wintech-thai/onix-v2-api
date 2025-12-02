using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMAccountDoc : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
    }
}
