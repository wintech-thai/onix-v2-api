using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVPortalPage
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MPortalPage? PortalPage { get; set; }
    }
}
