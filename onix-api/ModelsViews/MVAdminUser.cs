using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVAdminUser
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public MAdminUser? AdminUser { get; set; }
        public MUser? User { get; set; }
    }
}
