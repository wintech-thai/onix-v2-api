using System.Diagnostics.CodeAnalysis;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.ModelsViews
{
    [ExcludeFromCodeCoverage]
    public class MVOrganizationUserRegistration
    {
        public string? Status { get; set; }
        public string? Description { get; set; }
        public string? RegistrationUrl { get; set; }
    }
}
