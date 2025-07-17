using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVItemImage
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public List<MItemImage>? ItemImages { get; set; }
        public MItemImage? ItemImage { get; set; }
    }
}
