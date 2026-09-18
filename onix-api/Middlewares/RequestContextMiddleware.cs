using System.Security.Claims;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;

public class RequestContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private string? GetValue(HttpContext context, string key, string defaultValue)
    {
        var value = context.Items[key];
        if (value == null)
        {
            return defaultValue;
        }

        return value.ToString();
    }

    public async Task InvokeAsync(
        HttpContext context,
        RequestContext requestContext,
        IConfigurationService configurationService)
    {
        var cfClientIp = "";
        if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfConnectingIp))
        {
            cfClientIp = cfConnectingIp.ToString();
        }

        var clientIp = "";
        if (context.Request.Headers.TryGetValue("X-Original-Forwarded-For", out var xForwardedFor))
        {
            clientIp = xForwardedFor.ToString().Split(',')[0].Trim();
        }

        requestContext.IpAddress = string.Join(",",
            new[] { cfClientIp, clientIp }.Where(x => !string.IsNullOrWhiteSpace(x)));

        // Same IP resolution the blacklist feature uses (admin-configurable ClientIpSource, scope "Api"),
        // so audit trails show the same IP that actually drove any blacklist decision.
        requestContext.IpAddress2 = await ServiceUtils.ResolveConfiguredClientIp(context.Request, configurationService);

        var pc = ServiceUtils.GetPathComponent(context.Request);

        requestContext.OrgId = pc.OrgId;
        requestContext.ApiName = pc.ApiName;
        requestContext.RequestPath = context.Request.Path;
        requestContext.ActionBy = context.User.FindFirst(ClaimTypes.Name)?.Value;

        await _next(context);
    }
}
