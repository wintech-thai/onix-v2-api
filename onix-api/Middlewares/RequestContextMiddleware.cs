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
        RequestContext requestContext)
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

        requestContext.IpAddress = cfClientIp;
        requestContext.IpAddress2 = clientIp;

        var pc = ServiceUtils.GetPathComponent(context.Request);

        requestContext.OrgId = pc.OrgId;
        requestContext.RequestPath = context.Request.Path;
        requestContext.ActionBy = GetValue(context, "Temp-Identity-Name", "");

        await _next(context);
    }
}
