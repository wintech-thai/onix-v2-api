using System.Text.Json;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;

public class BlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public BlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        RequestContext requestContext,
        IOrganizationService organizationService,
        IConfigurationService configurationService)
    {
        // Our own Admin/Merchant backend relay attaches this header (from its own MUTUAL_KEY
        // env var) on every request it forwards to us. A correct value proves the request
        // genuinely came from our trusted relay (internal-to-internal), so it can skip the
        // blacklist check entirely. A present-but-wrong value means someone is trying to
        // impersonate the relay — treat that as blacklisted outright. No header at all means
        // this is a direct API call (e.g. a merchant integration hitting onix-api with an API
        // key) — fall through to the normal blacklist check.
        var mutualKeyHeader = context.Request.Headers["X-Forward-Mutual-Key"].ToString();
        if (!string.IsNullOrEmpty(mutualKeyHeader))
        {
            var expectedMutualKey = Environment.GetEnvironmentVariable("MUTUAL_KEY");
            if (!string.IsNullOrEmpty(expectedMutualKey) && mutualKeyHeader == expectedMutualKey)
            {
                await _next(context);
                return;
            }

            await WriteBlockedResponse(context, "INVALID_MUTUAL_KEY", "X-Forward-Mutual-Key header value does not match.", null, null, null);
            return;
        }

        var orgId = requestContext.OrgId;
        if (string.IsNullOrEmpty(orgId))
        {
            // Request path doesn't belong to a merchant (e.g. login, health check) — nothing to check.
            await _next(context);
            return;
        }

        // The merchant web page calls this endpoint to find out whether it is blacklisted.
        // It must never be blocked by the API-side check itself, or a client blacklisted on
        // the API list could never learn (and see) their own Web blacklist status.
        if (string.Equals(requestContext.ApiName, "GetIpPolicyStatus", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var clientIp = await ServiceUtils.ResolveConfiguredClientIp(context.Request, configurationService);
        var result = await organizationService.CheckIpBlacklist(orgId, clientIp, isApi: true);

        if (result.IsBlacklisted)
        {
            await WriteBlockedResponse(context, result.Status, result.Description, result.ClientIp, result.WhitelistIps, result.BlacklistIps);
            return;
        }

        await _next(context);
    }

    private static async Task WriteBlockedResponse(
        HttpContext context, string? status, string? description, string? clientIp, string? whitelistIps, string? blacklistIps)
    {
        context.Response.StatusCode = 422;
        context.Response.ContentType = "application/json";

        // Serialized with explicit camelCase keys — this is a manual JsonSerializer.Serialize
        // call, not routed through ASP.NET's configured MVC JSON options, so it would
        // otherwise come out PascalCase and silently mismatch what the frontend's error
        // parsing (and the rest of this API's camelCase responses) expects.
        var body = JsonSerializer.Serialize(new
        {
            status,
            description,
            clientIp,
            whitelistIps,
            blacklistIps,
        });

        await context.Response.WriteAsync(body);
    }
}
