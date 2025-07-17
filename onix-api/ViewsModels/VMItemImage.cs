using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMItemImage : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public int? Category { get; set; }
        public string? ItemId { get; set; }
    }
}
