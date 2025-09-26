using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Test.Services;

public class AdminServiceTest
{
    [Fact]
    public void IsOrganizationExistTest()
    {
        var orgSvc = new Mock<IOrganizationService>();
        orgSvc.Setup(s => s.IsOrgIdExist("orgtest")).Returns(true);

        var userSvc = new Mock<IUserService>();
        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();
        var redisHelper = new Mock<IRedisHelper>();

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);
        var orgExistResult = adminSvc.IsOrganizationExist("orgtest");

        Assert.True(orgExistResult);
    }

    [Fact]
    public void IsOrganizationNotExistTest()
    {
        var orgSvc = new Mock<IOrganizationService>();
        orgSvc.Setup(s => s.IsOrgIdExist("orgtest")).Returns(true);

        var userSvc = new Mock<IUserService>();
        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();
        var redisHelper = new Mock<IRedisHelper>();

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);
        var orgExistResult = adminSvc.IsOrganizationExist("xxxssaaa");

        Assert.False(orgExistResult);
    }

    [Fact]
    public void IsUserNameExistTest()
    {
        var orgSvc = new Mock<IOrganizationService>();

        var userSvc = new Mock<IUserService>();
        userSvc.Setup(s => s.IsUserNameExist("notused", "user1")).Returns(true);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();
        var redisHelper = new Mock<IRedisHelper>();

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);
        var userExistResult = adminSvc.IsUserNameExist("user1");

        Assert.True(userExistResult);
    }

    [Fact]
    public void IsUserNameNotExistTest()
    {
        var orgSvc = new Mock<IOrganizationService>();

        var userSvc = new Mock<IUserService>();
        userSvc.Setup(s => s.IsUserNameExist("notused", "user1")).Returns(true);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();
        var redisHelper = new Mock<IRedisHelper>();

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);
        var userExistResult = adminSvc.IsUserNameExist("user2");

        Assert.False(userExistResult);
    }

    [Fact]
    public void IsEmailExistTest()
    {
        var orgSvc = new Mock<IOrganizationService>();

        var userSvc = new Mock<IUserService>();
        userSvc.Setup(s => s.IsEmailExist("notused", "test@xxx.com")).Returns(true);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();
        var redisHelper = new Mock<IRedisHelper>();

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);
        var emailExistResult = adminSvc.IsEmailExist("test@xxx.com");

        Assert.True(emailExistResult);
    }

    [Fact]
    public void IsEmailNotExistTest()
    {
        var orgSvc = new Mock<IOrganizationService>();

        var userSvc = new Mock<IUserService>();
        userSvc.Setup(s => s.IsEmailExist("notused", "no-reply@xxx.com")).Returns(true);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();
        var redisHelper = new Mock<IRedisHelper>();

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);
        var emailExistResult = adminSvc.IsEmailExist("test@xxx.com");

        Assert.False(emailExistResult);
    }
}