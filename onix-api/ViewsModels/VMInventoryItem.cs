using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMInventoryItem : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public string? ItemType { get; set; }
    }
}
