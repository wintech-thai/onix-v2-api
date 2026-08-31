using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVOrganizationPolicy
    {
        public string? Status { get; set; }
        public string? Description { get; set; }

        public MOrganizationPolicy? OrganizationPolicy { get; set; }

        public MVOrganizationPolicy()
        {
        }
    }
}
