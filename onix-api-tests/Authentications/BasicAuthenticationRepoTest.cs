using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;
using Its.Onix.Api.Authentications;
using Microsoft.AspNetCore.Http;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Models;

namespace Its.Onix.Api.Test.Authentications;

public class BasicAuthenticationRepoTest
{
    [Theory]
    [InlineData("temp", "secret1", "/api/User/org/temp/action/ThisIsApiAxxx")]
    [InlineData("temp", "secret2", "/api/Axxxxx/org/temp/action/UpdatePassword")]
    public void AuthenticateBasicErrorTest(string orgId, string apiKey, string path)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Path = path;

        var apiKeySvc = new Mock<IApiKeyService>();
        // Simulate ไม่พบ api key ใน DB
        apiKeySvc.Setup(s => s.VerifyApiKey(orgId, apiKey)).Returns((MVApiKey)null!);

        var redisHelper = new Mock<IRedisHelper>();
        var cacheKey = $"{orgId}:VerifyKey:{apiKey}";
        // Simulate ไม่พบ api key ใน Redis
        redisHelper.Setup(s => s.GetObjectAsync<MVApiKey>(cacheKey)).ReturnsAsync((MVApiKey)null!);

        var authRepo = new BasicAuthenticationRepo(apiKeySvc.Object, redisHelper.Object);
        var user = authRepo.Authenticate(orgId, "user1", apiKey, request);

        Assert.Null(user);
    }
    
    [Theory]
    [InlineData("temp", "secret1", "/api/User/org/temp/action/ThisIsApiAxxx")]
    [InlineData("temp", "secret2", "/api/Axxxxx/org/temp/action/UpdatePassword")]
    public void AuthenticateBasicOkTest(string orgId, string apiKey, string path)
    {
        var mockedUser = "user1";
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Path = path;

        var apiKeySvc = new Mock<IApiKeyService>();
        // Simulate ว่าพบ api key ใน DB
        apiKeySvc.Setup(s => s.VerifyApiKey(orgId, apiKey)).Returns(new MVApiKey()
        {
            ApiKey = new MApiKey()
            {
                ApiKey = apiKey,
                OrgId = orgId,
                RolesList = "OWNER",
            } ,
            Status = "SUCCESS",
        });

        var redisHelper = new Mock<IRedisHelper>();
        var cacheKey = $"{orgId}:VerifyKey:{apiKey}";
        // Simulate ไม่พบ api key ใน Redis
        redisHelper.Setup(s => s.GetObjectAsync<MVApiKey>(cacheKey)).ReturnsAsync((MVApiKey) null!);

        var authRepo = new BasicAuthenticationRepo(apiKeySvc.Object, redisHelper.Object);
        var user = authRepo.Authenticate(orgId, mockedUser, apiKey, request);

        Assert.NotNull(user);
        Assert.NotNull(user.Claims);

        Assert.Equal(mockedUser, user.UserName);
        Assert.Equal("API-KEY", user.AuthenType);
        Assert.True(user.Claims.ToArray().Length > 0);
    }
}
