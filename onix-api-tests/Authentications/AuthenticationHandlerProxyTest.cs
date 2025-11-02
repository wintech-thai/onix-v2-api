using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Authentications;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text;

namespace Its.Onix.Api.Test.Authentications;

public class AuthenticationHandlerProxyTest
{
    [Theory]
    [InlineData("/health")]
    [Obsolete]
    public async Task AuthenticateHealthCheckTest(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        var options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Setup(o => o.Get(It.IsAny<string>())).Returns(new AuthenticationSchemeOptions());

        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;
        var clock = new SystemClock();

        // Mock repository/service
        var basicRepo = new Mock<IBasicAuthenticationRepo>();
        var bearerRepo = new Mock<IBearerAuthenticationRepo>();
        var authService = new Mock<IAuthService>();

        var handler = new AuthenticationHandlerProxy(
            options.Object,
            loggerFactory,
            encoder,
            basicRepo.Object,
            bearerRepo.Object,
            authService.Object,
            clock
        );

        await handler.InitializeAsync(
            new AuthenticationScheme("TestScheme", "TestScheme", typeof(AuthenticationHandlerProxy)),
            context
        );

        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("/api/User/org/temp/action/ThisIsApiAxxx")]
    [Obsolete]
    public async Task AuthenticateNoAuthorizationHeaderTest(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        var options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Setup(o => o.Get(It.IsAny<string>())).Returns(new AuthenticationSchemeOptions());

        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;
        var clock = new SystemClock();

        // Mock repository/service
        var basicRepo = new Mock<IBasicAuthenticationRepo>();
        var bearerRepo = new Mock<IBearerAuthenticationRepo>();
        var authService = new Mock<IAuthService>();

        var handler = new AuthenticationHandlerProxy(
            options.Object,
            loggerFactory,
            encoder,
            basicRepo.Object,
            bearerRepo.Object,
            authService.Object,
            clock
        );

        await handler.InitializeAsync(
            new AuthenticationScheme("TestScheme", "TestScheme", typeof(AuthenticationHandlerProxy)),
            context
        );

        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
    }

    [Theory]
    [InlineData("/api/User/org/temp/action/ThisIsApiAxxx")]
    [Obsolete]
    public async Task AuthenticateUnknownSchemeTest(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Headers["Authorization"] = "XTestXX faketoken123";

        var options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Setup(o => o.Get(It.IsAny<string>())).Returns(new AuthenticationSchemeOptions());

        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;
        var clock = new SystemClock();

        // Mock repository/service
        var basicRepo = new Mock<IBasicAuthenticationRepo>();
        var bearerRepo = new Mock<IBearerAuthenticationRepo>();
        var authService = new Mock<IAuthService>();

        var handler = new AuthenticationHandlerProxy(
            options.Object,
            loggerFactory,
            encoder,
            basicRepo.Object,
            bearerRepo.Object,
            authService.Object,
            clock
        );

        await handler.InitializeAsync(
            new AuthenticationScheme("TestScheme", "TestScheme", typeof(AuthenticationHandlerProxy)),
            context
        );

        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
    }

    [Theory]
    [InlineData("/api/User/org/temp/action/ThisIsApiAxxx")]
    [Obsolete]
    public async Task AuthenticateWrongBase64Test(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Headers["Authorization"] = "Basic thisiswrongbase64==";

        var options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Setup(o => o.Get(It.IsAny<string>())).Returns(new AuthenticationSchemeOptions());

        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;
        var clock = new SystemClock();

        // Mock repository/service
        var basicRepo = new Mock<IBasicAuthenticationRepo>();
        var bearerRepo = new Mock<IBearerAuthenticationRepo>();
        var authService = new Mock<IAuthService>();

        var handler = new AuthenticationHandlerProxy(
            options.Object,
            loggerFactory,
            encoder,
            basicRepo.Object,
            bearerRepo.Object,
            authService.Object,
            clock
        );

        await handler.InitializeAsync(
            new AuthenticationScheme("TestScheme", "TestScheme", typeof(AuthenticationHandlerProxy)),
            context
        );

        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
    }

    [Theory]
    [InlineData("/api/User/org/temp/action/ThisIsApiAxxx")]
    [Obsolete]
    public async Task AuthenticateBasicErrorTest(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        var key = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("user1:password"));
        context.Request.Headers["Authorization"] = $"Basic {key}";

        var options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Setup(o => o.Get(It.IsAny<string>())).Returns(new AuthenticationSchemeOptions());

        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;
        var clock = new SystemClock();

        // Mock repository/service
        var basicRepo = new Mock<IBasicAuthenticationRepo>();
        var bearerRepo = new Mock<IBearerAuthenticationRepo>();
        var authService = new Mock<IAuthService>();

        var handler = new AuthenticationHandlerProxy(
            options.Object,
            loggerFactory,
            encoder,
            basicRepo.Object,
            bearerRepo.Object,
            authService.Object,
            clock
        );

        await handler.InitializeAsync(
            new AuthenticationScheme("TestScheme", "TestScheme", typeof(AuthenticationHandlerProxy)),
            context
        );

        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
    }

    [Theory]
    [InlineData("/api/User/org/temp/action/ThisIsApiAxxx")]
    [Obsolete]
    public async Task AuthenticateBearerErrorTest(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        var jwt = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}.{\"sub\":\"1234567890\",\"name\":\"John Doe\",\"iat\":1516239022}.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"));
        context.Request.Headers["Authorization"] = $"Bearer {jwt}";

        var options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Setup(o => o.Get(It.IsAny<string>())).Returns(new AuthenticationSchemeOptions());

        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;
        var clock = new SystemClock();

        // Mock repository/service
        var basicRepo = new Mock<IBasicAuthenticationRepo>();
        var bearerRepo = new Mock<IBearerAuthenticationRepo>();
        var authService = new Mock<IAuthService>();

        var handler = new AuthenticationHandlerProxy(
            options.Object,
            loggerFactory,
            encoder,
            basicRepo.Object,
            bearerRepo.Object,
            authService.Object,
            clock
        );

        await handler.InitializeAsync(
            new AuthenticationScheme("TestScheme", "TestScheme", typeof(AuthenticationHandlerProxy)),
            context
        );

        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
    }

    [Theory]
    [InlineData("temp", "user1", "/api/User/org/temp/action/ThisIsApiAxxx")]
    [InlineData("temp", "user2", "/api/Axxxxx/org/temp/action/UpdatePassword")]
    [Obsolete]
    public async Task AuthenticateBasicOkTest(string orgId, string userName, string path)
    {
        var password = "helloworld";
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        var key = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{userName}:{password}"));
        context.Request.Headers["Authorization"] = $"Basic {key}";

        var options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Setup(o => o.Get(It.IsAny<string>())).Returns(new AuthenticationSchemeOptions());

        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;
        var clock = new SystemClock();

        // Mock repository/service
        var basicRepo = new Mock<IBasicAuthenticationRepo>();
        var bearerRepo = new Mock<IBearerAuthenticationRepo>();
        var authService = new Mock<IAuthService>();

        // Stub method ของ repo ตามเงื่อนไข
        basicRepo.Setup(r => r.Authenticate(orgId, userName, password, context.Request))
            .Returns(new User
            {
                UserName = userName,
                Role = "OWNER",
                AuthenType = "API-KEY",
                OrgId = orgId,
                UserId = Guid.NewGuid(),
                Email = "",
                Status = "OK",
            });

        var handler = new AuthenticationHandlerProxy(
            options.Object,
            loggerFactory,
            encoder,
            basicRepo.Object,
            bearerRepo.Object,
            authService.Object,
            clock
        );

        await handler.InitializeAsync(
            new AuthenticationScheme("TestScheme", "TestScheme", typeof(AuthenticationHandlerProxy)),
            context
        );

        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
    }

    private string CreateAccessToken(string userName)
    { 
        var header = new { alg = "HS256", typ = "JWT" };
        var payload = new
        {
            sub = "1234567890",
            name = "John Doe",
            preferred_username = userName,
            iat = 1516239022
        };

        string headerJson = JsonSerializer.Serialize(header);
        string payloadJson = JsonSerializer.Serialize(payload);

        string headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        string payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

        // ใช้ dummy signature แทน
        string signature = Base64UrlEncode(Encoding.UTF8.GetBytes("dummy-signature"));

        string jwt = $"{headerBase64}.{payloadBase64}.{signature}";
        return jwt;
    }

    static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    [Theory]
    [InlineData("temp", "user1", "/api/User/org/temp/action/ThisIsApiAxxx")]
    [InlineData("temp", "user2", "/api/Axxxxx/org/temp/action/UpdatePassword")]
    [Obsolete]
    public async Task AuthenticateBearerOkTest(string orgId, string userName, string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        var accessToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(CreateAccessToken(userName)));
        context.Request.Headers["Authorization"] = $"Bearer {accessToken}";

        var options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Setup(o => o.Get(It.IsAny<string>())).Returns(new AuthenticationSchemeOptions());

        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;
        var clock = new SystemClock();

        // Mock repository/service
        var basicRepo = new Mock<IBasicAuthenticationRepo>();
        var bearerRepo = new Mock<IBearerAuthenticationRepo>();
        var authService = new Mock<IAuthService>();

        // ใน code จะส่ง empty string เป็น password มาให้ method Authenticate ของ bearerRepo
        bearerRepo.Setup(r => r.Authenticate(orgId, userName, "", context.Request))
            .Returns(new User
            {
                UserName = userName,
                Role = "OWNER",
                AuthenType = "JWT",
                OrgId = orgId,
                UserId = Guid.NewGuid(),
                Email = "",
                Status = "OK",
            });

        var handler = new AuthenticationHandlerProxy(
            options.Object,
            loggerFactory,
            encoder,
            basicRepo.Object,
            bearerRepo.Object,
            authService.Object,
            clock
        );

        await handler.InitializeAsync(
            new AuthenticationScheme("TestScheme", "TestScheme", typeof(AuthenticationHandlerProxy)),
            context
        );

        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
    }
    
    [Theory]
    [InlineData("temp", "user1", "/api/User/org/temp/action/ThisIsApiAxxx")]
    [InlineData("temp", "user2", "/api/Axxxxx/org/temp/action/UpdatePassword")]
    [Obsolete]
    public async Task AuthenticateBearerStatusNotOkTest(string orgId, string userName, string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        var accessToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(CreateAccessToken(userName)));
        context.Request.Headers["Authorization"] = $"Bearer {accessToken}";

        var options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Setup(o => o.Get(It.IsAny<string>())).Returns(new AuthenticationSchemeOptions());

        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;
        var clock = new SystemClock();

        // Mock repository/service
        var basicRepo = new Mock<IBasicAuthenticationRepo>();
        var bearerRepo = new Mock<IBearerAuthenticationRepo>();
        var authService = new Mock<IAuthService>();

        // ใน code จะส่ง empty string เป็น password มาให้ method Authenticate ของ bearerRepo
        bearerRepo.Setup(r => r.Authenticate(orgId, userName, "", context.Request))
            .Returns(new User
            {
                UserName = userName,
                Role = "OWNER",
                AuthenType = "JWT",
                OrgId = orgId,
                UserId = Guid.NewGuid(),
                Email = "",
                Status = "ERROR",
            });

        var handler = new AuthenticationHandlerProxy(
            options.Object,
            loggerFactory,
            encoder,
            basicRepo.Object,
            bearerRepo.Object,
            authService.Object,
            clock
        );

        await handler.InitializeAsync(
            new AuthenticationScheme("TestScheme", "TestScheme", typeof(AuthenticationHandlerProxy)),
            context
        );
        
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
    }
}
