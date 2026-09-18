using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    // Deliberately routed under /public-api/* — NOT /admin-api/* or /api/* — so
    // RequestContextMiddleware.GetPathComponent() never resolves an OrgId for these
    // routes, which means BlacklistMiddleware passes them through unconditionally.
    // Branding assets (logo, name, theme) must always be servable regardless of any
    // IP whitelist/blacklist policy — they're not an admin action, just a public asset.
    [AllowAnonymous]
    [ApiController]
    [Route("/public-api/[controller]")]
    public class PublicBrandingController : ControllerBase
    {
        private readonly IConfigurationService svc;

        [ExcludeFromCodeCoverage]
        public PublicBrandingController(IConfigurationService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("action/GetBrandConfig")]
        public async Task<IActionResult> GetBrandConfig()
        {
            var result = await svc.GetBrandConfig("global", true);
            return Ok(result);
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("action/GetBrandLogoImage")]
        public async Task<IActionResult> GetBrandLogoImage()
        {
            var (bytes, mimeType) = await svc.GetBrandLogoImageBytes("global");
            if (bytes == null)
            {
                return NotFound();
            }

            // Static path, no signature/query — must prevent browser/CDN caching so a
            // newly-uploaded logo doesn't keep showing the old one (or a stale 404).
            Response.Headers.CacheControl = "no-store";

            return File(bytes, string.IsNullOrEmpty(mimeType) ? "image/png" : mimeType);
        }
    }
}
