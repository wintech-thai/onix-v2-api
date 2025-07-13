using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMItem : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public int? ItemType { get; set; }
    }
}
