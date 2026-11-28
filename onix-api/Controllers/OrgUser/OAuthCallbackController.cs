using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.Services;

namespace Its.Onix.Api.Controllers
{
    // Google redirect กลับมาเรียกตรงนี้ (public, ไม่มี auth header)
    [AllowAnonymous]
    [ApiController]
    [Route("/api/[controller]")]
    public class OAuthCallbackController : ControllerBase
    {
        private readonly IPortalSessionService svc;

        [ExcludeFromCodeCoverage]
        public OAuthCallbackController(IPortalSessionService service)
        {
            svc = service;
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/{id}/action/GoogleCallback")]
        public async Task<IActionResult> GoogleCallback(string id, [FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error)
        {
            var result = await svc.HandleGoogleCallback(id, code, state, error);

            if (result.Status == "OK")
            {
                if (!string.IsNullOrEmpty(result.RedirectUrl))
                {
                    return Redirect(result.RedirectUrl);
                }

                return Content(BuildHtmlPage("Signed in", "You have signed in successfully. You may now close this page.", isError: false), "text/html");
            }

            return Content(BuildHtmlPage("Sign-in failed", result.Description ?? "Google sign-in failed", isError: true), "text/html");
        }

        private static string BuildHtmlPage(string title, string message, bool isError)
        {
            var accent = isError ? "#f87171" : "#34d399";
            return $@"<!doctype html>
<html>
<head><meta charset=""utf-8""><title>{WebUtility.HtmlEncode(title)}</title></head>
<body style=""font-family:-apple-system,'Segoe UI',Roboto,sans-serif;display:flex;align-items:center;justify-content:center;min-height:100vh;margin:0;background:#0f172a;color:#fff;"">
  <div style=""text-align:center;max-width:420px;padding:24px;"">
    <h2 style=""color:{accent};margin:0 0 12px;"">{WebUtility.HtmlEncode(title)}</h2>
    <p style=""color:#94a3b8;margin:0;"">{WebUtility.HtmlEncode(message)}</p>
  </div>
</body>
</html>";
        }
    }
}
