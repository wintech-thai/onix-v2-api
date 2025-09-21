using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Services
{
    public interface IAdminService
    {
        public MVOtp SendOrgRegisterOtpEmail(string orgId, string email);
        public MVOrganizeRegistration RegisterOrganization(string orgId, MOrganizeRegistration user);
    }
}
