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
    [Route("/api/OauthCallback")]
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
            var iconBg = isError ? "rgba(248,113,113,.14)" : "rgba(52,211,153,.14)";
            var icon = isError
                ? @"<svg width=""30"" height=""30"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#f87171"" stroke-width=""2.5"" stroke-linecap=""round"" stroke-linejoin=""round""><line x1=""18"" y1=""6"" x2=""6"" y2=""18""/><line x1=""6"" y1=""6"" x2=""18"" y2=""18""/></svg>"
                : @"<svg width=""30"" height=""30"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#34d399"" stroke-width=""2.5"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M20 6L9 17l-5-5""/></svg>";

            return $@"<!doctype html>
<html>
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<title>{WebUtility.HtmlEncode(title)}</title>
<style>
  * {{ box-sizing: border-box; }}
  body {{
    font-family: -apple-system,'Segoe UI',Roboto,sans-serif;
    display: flex; align-items: center; justify-content: center;
    min-height: 100vh; margin: 0;
    background: radial-gradient(circle at top, #1e293b 0%, #0f172a 60%);
    color: #fff;
  }}
  .card {{
    background: #111827;
    border: 1px solid rgba(255,255,255,.08);
    border-radius: 16px;
    box-shadow: 0 20px 60px rgba(0,0,0,.45);
    text-align: center;
    max-width: 420px; width: 100%;
    padding: 40px 32px;
    animation: pop .35s ease-out;
  }}
  @keyframes pop {{ from {{ opacity: 0; transform: translateY(8px) scale(.98); }} to {{ opacity: 1; transform: none; }} }}
  .icon {{
    width: 64px; height: 64px; margin: 0 auto 20px;
    border-radius: 50%;
    display: flex; align-items: center; justify-content: center;
    background: {iconBg};
  }}
  h2 {{ margin: 0 0 8px; font-size: 20px; color: #f9fafb; }}
  p {{ margin: 0; font-size: 14px; color: #94a3b8; line-height: 1.5; }}
</style>
</head>
<body>
  <div class=""card"">
    <div class=""icon"">{icon}</div>
    <h2 style=""color:{accent};"">{WebUtility.HtmlEncode(title)}</h2>
    <p>{WebUtility.HtmlEncode(message)}</p>
  </div>
</body>
</html>";
        }
    }
}
