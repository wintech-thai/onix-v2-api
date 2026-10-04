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

        // Loki is installed cluster-wide already (same "loki-log" service please-protect-api's
        // ProxyController.Loki() points at). Unlike _promClient this doesn't go through
        // IHttpClientFactory/Program.cs registration — a plain HttpClient is enough for a
        // single internal, unauthenticated backend and keeps this feature self-contained.
        private static readonly HttpClient _lokiClient = new()
        {
            BaseAddress = new Uri(Environment.GetEnvironmentVariable("LOKI_URL")
                ?? "http://loki-log.loki-log.svc.cluster.local:3100"),
            Timeout = TimeSpan.FromSeconds(30),
        };

        private static readonly string[] AllowedPrometheusPrefixes =
        {
            "api/v1/query",
            "api/v1/query_range",
            "api/v1/series",
            "api/v1/labels",
            "api/v1/label",
        };

        // Mirrors please-protect-api's ProxyController.Loki() blocklist — Loki's write/ingest
        // endpoints must never be reachable through this read-only log viewer proxy.
        private static readonly string[] BlockedLokiPrefixes =
        {
            "api/v1/push",
            "api/prom/push",
            "api/v1/delete",
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

        [ExcludeFromCodeCoverage]
        [AcceptVerbs("GET", "POST")]
        [Route("org/global/action/Loki/{**path}")]
        public async Task Loki(string path, CancellationToken ct)
        {
            path ??= "";

            if (BlockedLokiPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                Response.StatusCode = StatusCodes.Status403Forbidden;
                await Response.WriteAsync("API not allowed");
                return;
            }

            var targetUri = $"{path}{Request.QueryString}";

            using var requestMessage = new HttpRequestMessage(new HttpMethod(Request.Method), targetUri);

            // Don't forward Host/Authorization — this is an internal, unauthenticated
            // Loki service; our own [Authorize] above already gated the caller.
            foreach (var header in Request.Headers)
            {
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    continue;

                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            using var responseMessage = await _lokiClient.SendAsync(
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
