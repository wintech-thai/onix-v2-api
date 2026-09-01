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
        IOrganizationService organizationService)
    {
        var orgId = requestContext.OrgId;
        if (string.IsNullOrEmpty(orgId))
        {
            // Request path doesn't belong to a merchant (e.g. login, health check) — nothing to check.
            await _next(context);
            return;
        }

        var clientIp = ServiceUtils.GetClientIp(context.Request);
        var result = await organizationService.CheckIpBlacklist(orgId, clientIp, isApi: true);

        if (result.IsBlacklisted)
        {
            context.Response.StatusCode = 422;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new
            {
                Status = result.Status,
                Description = result.Description,
                ClientIp = result.ClientIp,
                WhitelistIps = result.WhitelistIps,
                BlacklistIps = result.BlacklistIps,
            });

            await context.Response.WriteAsync(body);
            return;
        }

        await _next(context);
    }
}
