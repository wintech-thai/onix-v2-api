using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Controllers
{
    // Deliberately routed under /public-api/* — NOT /admin-api/* or /api/* — so
    // RequestContextMiddleware.GetPathComponent() never resolves an OrgId for these
    // routes, which means BlacklistMiddleware passes them through unconditionally.
    // This endpoint answers "is my IP even allowed to load the Admin website at all"
    // for a visitor who, by definition, has no auth token yet (the check must run
    // before the login page itself renders) — so it cannot sit behind [Authorize]
    // like the existing OrgUser GetIpPolicyStatus does.
    [AllowAnonymous]
    [ApiController]
    [Route("/public-api/[controller]")]
    public class PublicOrganizationController : ControllerBase
    {
        private readonly IOrganizationService orgSvc;
        private readonly IConfigurationService configSvc;

        [ExcludeFromCodeCoverage]
        public PublicOrganizationController(IOrganizationService organizationService, IConfigurationService configurationService)
        {
            orgSvc = organizationService;
            configSvc = configurationService;
        }

        // Admin's own Web IP policy always lives under org "global" — there is no
        // per-visitor org to resolve before login, unlike the per-merchant policies
        // merchant users see post-login via the OrgUser controller.
        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("action/GetAdminWebIpPolicyStatus")]
        public async Task<IActionResult> GetAdminWebIpPolicyStatus()
        {
            var clientIp = await ServiceUtils.ResolveConfiguredClientIp(Request, configSvc);
            var result = await orgSvc.CheckIpBlacklist("global", clientIp, isApi: false);
            return Ok(result);
        }
    }
}
