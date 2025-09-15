using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVItem
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MItem? Item { get; set; }
        public ICollection<MImage> Images { get; set; } = new List<MImage>();
    }
}
