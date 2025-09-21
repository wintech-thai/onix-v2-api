using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.Models
{
    [ExcludeFromCodeCoverage]
    public class MOrganizeRegistration
    {
        public string ProofEmailOtp { get; set; }
        public string? UserOrgName { get; set; }
        public string? UserName { get; set; }
        public string UserInitialPassword { get; set; } /* Pass to Keycloak */
        public string? Name { get; set; } /* Pass to Keycloak */
        public string? Lastname { get; set; } /* Pass to Keycloak */
        public string? Email { get; set; } /* Pass to Keycloak */
/*
        public MOrganization? Organization { get; set; }
        public MUser? InitialUser { get; set; }
*/
        public MOrganizeRegistration()
        {
            ProofEmailOtp = "";
            UserInitialPassword = "";
            //Organization = new MOrganization();
            //InitialUser = new MUser();
        }
    }
}
