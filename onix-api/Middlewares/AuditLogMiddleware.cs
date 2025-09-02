using System.Diagnostics;
using System.Text.Json;
using Serilog;

namespace Its.Onix.Api.AuditLogs
{
    public class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            var scheme = context.Request.Scheme;
            var method = context.Request.Method;
            var host = context.Request.Headers["X-Forwarded-Host"].ToString();
            var path = context.Request.Path;
            var query = context.Request.QueryString.ToString();
            var fullUrl = $"{method} {path}{query}";
            var requestSize = context.Request.ContentLength ?? 0;
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            var cfClientIp = "";
            if (context.Request.Headers.ContainsKey("CF-Connecting-IP"))
            {
                cfClientIp = context.Request.Headers["CF-Connecting-IP"].ToString();
            }

            var clientIp = "";
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var xForwardedFor))
            {
                clientIp = xForwardedFor.ToString(); //.Split(',')[0].Trim();
            }

            await _next(context); // call next middleware

            var responseSize = memoryStream.Length;
            var statusCode = context.Response.StatusCode;

            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

            stopwatch.Stop();

            var latencyMs = stopwatch.ElapsedMilliseconds;

            // === Build log JSON ===
            var logObject = new AuditLog()
            {
                Host = host,
                HttpMethod = method,
                StatusCode = statusCode,
                Path = path,
                QueryString = query,
                UserAgent = userAgent,
                RequestSize = requestSize,
                ResponseSize = responseSize,
                LatencyMs = latencyMs,
                Scheme = scheme,
                ClientIp = clientIp,
                CfClientIp = cfClientIp,
            };

            var logJson = JsonSerializer.Serialize(logObject);
            Log.Information(logJson);
        }
    }
}
