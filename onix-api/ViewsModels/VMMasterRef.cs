using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMMasterRef : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public int? RefType { get; set; }
    }
}
