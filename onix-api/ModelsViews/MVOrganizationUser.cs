using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVOrganizationUser
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MOrganizationUser? OrgUser { get; set; }
        public MUser? User { get; set; }
    }
}
