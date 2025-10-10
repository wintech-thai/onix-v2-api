using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVUpdateSortingOrder
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public IEnumerable<string>? Ids { get; set; }
    }
}
