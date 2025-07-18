using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMEntity : VMQueryBase
    {
        public string? FullTextSearch { get; set; }
        public int? EntityType { get; set; }
        public int? EntityCategory { get; set; }
    }
}
