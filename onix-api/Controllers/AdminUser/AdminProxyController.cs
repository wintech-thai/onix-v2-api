using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Its.Onix.Api.Controllers
{
    // Same pattern as please-protect-api's ProxyController.Prometheus() — a thin,
    // read-only-allowlisted passthrough so the Resource Monitoring dashboard can query
    // Prometheus (installed cluster-wide already) without exposing the whole Prometheus
    // API surface (which also has write/admin endpoints) directly to the internet.
    [Authorize(Policy = "GenericRolePolicy")]
    [ApiController]
    [Route("/admin-api/[controller]")]
    public class AdminProxyController : ControllerBase
    {
        private readonly HttpClient _promClient;

        private static readonly string[] AllowedPrometheusPrefixes =
        {
            "api/v1/query",
            "api/v1/query_range",
            "api/v1/series",
            "api/v1/labels",
            "api/v1/label",
        };

        [ExcludeFromCodeCoverage]
        public AdminProxyController(IHttpClientFactory factory)
        {
            _promClient = factory.CreateClient("prom-proxy");
        }

        [ExcludeFromCodeCoverage]
        [HttpGet]
        [Route("org/global/action/Prometheus/{**path}")]
        public async Task Prometheus(string path, CancellationToken ct)
        {
            path ??= "";

            if (!AllowedPrometheusPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                Response.StatusCode = StatusCodes.Status403Forbidden;
                await Response.WriteAsync("API not allowed");
                return;
            }

            var targetUri = $"{path}{Request.QueryString}";

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, targetUri);

            // Don't forward Host/Authorization — this is an internal, unauthenticated
            // Prometheus service; our own [Authorize] above already gated the caller.
            foreach (var header in Request.Headers)
            {
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    continue;

                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            using var responseMessage = await _promClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

            Response.StatusCode = (int)responseMessage.StatusCode;

            foreach (var h in responseMessage.Headers)
                Response.Headers[h.Key] = h.Value.ToArray();

            foreach (var h in responseMessage.Content.Headers)
                Response.Headers[h.Key] = h.Value.ToArray();

            Response.Headers.Remove("transfer-encoding");

            Response.ContentType = responseMessage.Content.Headers.ContentType?.ToString() ?? "application/json";

            await responseMessage.Content.CopyToAsync(Response.Body, ct);
        }
    }
}
