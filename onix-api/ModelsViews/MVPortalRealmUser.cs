using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVPortalRealmUser
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MPortalRealmUser? RealmUser { get; set; }
    }
}
