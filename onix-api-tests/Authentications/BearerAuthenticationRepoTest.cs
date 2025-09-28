using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;
using Its.Onix.Api.Authentications;
using Microsoft.AspNetCore.Http;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Test.Authentications;

public class BearerAuthenticationRepoTest
{
    [Theory]
    [InlineData("temp", "user1", "/api/OnlyUser/org/temp/action/UpdatePassword")]
    [InlineData("temp", "user1", "/api/OnlyUser/org/temp/action/GetUserAllowedOrg")]
    public void AuthenticateBearerWhiteListTest(string orgId, string userName, string path)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Path = path;

        var orgSvc = new Mock<IOrganizationService>();
        var redisHelper = new Mock<IRedisHelper>();

        var authRepo = new BearerAuthenticationRepo(orgSvc.Object, redisHelper.Object);
        var user = authRepo.Authenticate(orgId, userName, "", request);

        Assert.NotNull(user);
        Assert.NotNull(user.Claims);

        Assert.Equal(userName, user.UserName);
        Assert.Equal("JWT", user.AuthenType);
        Assert.True(user.Claims.ToArray().Length > 0);
    }

    [Theory]
    [InlineData("temp", "user1", "/api/User/org/temp/action/ThisIsApiAxxx")]
    [InlineData("temp", "user1", "/api/Axxxxx/org/temp/action/UpdatePassword")]
    public void AuthenticateBearerErrorTest(string orgId, string userName, string path)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Path = path;

        var orgSvc = new Mock<IOrganizationService>();

        var redisHelper = new Mock<IRedisHelper>();
        var cacheKey = $"{orgId}:VerifyUser:{userName}";
        redisHelper.Setup(s => s.GetObjectAsync<MVOrganizationUser>(It.IsAny<string>())).ReturnsAsync(new MVOrganizationUser()
        {
            Status = "ERROR"
        });

        var authRepo = new BearerAuthenticationRepo(orgSvc.Object, redisHelper.Object);
        var user = authRepo.Authenticate(orgId, userName, "", request);

        Assert.Null(user);
    }

    [Theory]
    [InlineData("temp", "user1", "/api/User/org/temp/action/ThisIsApiAxxx")]
    [InlineData("temp", "user1", "/api/Axxxxx/org/temp/action/UpdatePassword")]
    public void AuthenticateBearerUserNotFoundTest(string orgId, string userName, string path)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Path = path;

        var orgSvc = new Mock<IOrganizationService>();
        //Simulate ไม่พบ user ใน DB (OrganizationsUsers table)
        orgSvc.Setup(s => s.VerifyUserInOrganization(It.IsAny<string>(), It.IsAny<string>())).Returns((MVOrganizationUser) null!);

        var redisHelper = new Mock<IRedisHelper>();
        var cacheKey = $"{orgId}:VerifyUser:{userName}";
        //Simulate ไม่พบ user ใน Redis
        redisHelper.Setup(s => s.GetObjectAsync<MVOrganizationUser>(It.IsAny<string>())).ReturnsAsync((MVOrganizationUser) null!);

        var authRepo = new BearerAuthenticationRepo(orgSvc.Object, redisHelper.Object);
        var user = authRepo.Authenticate(orgId, userName, "", request);

        Assert.Null(user);
    }
}
