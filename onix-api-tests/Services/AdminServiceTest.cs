using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Utils;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;

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

    [Fact]
    public void SendOrgRegisterOtpEmailTest()
    {
        var orgSvc = new Mock<IOrganizationService>();
        var userSvc = new Mock<IUserService>();
        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();
        var redisHelper = new Mock<IRedisHelper>();

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);
        var result = adminSvc.SendOrgRegisterOtpEmail("org1", "test@gmail.com");

        Assert.True(result.OTP!.Length == 6); // OTP should be 6 characters
    }

    [Fact]
    public void RegisterOrganizationOtpNotFoundTest()
    {
        var orgSvc = new Mock<IOrganizationService>();
        var userSvc = new Mock<IUserService>();
        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();
        var redisHelper = new Mock<IRedisHelper>();

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);
        var usrRegister = new MOrganizeRegistration();

        var result = adminSvc.RegisterOrganization("org1", usrRegister);

        Assert.Equal("PROVIDED_OTP_NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "SendOrgRegisterOtpEmail", "test1@gmail.com", "123456", "PROVIDED_OTP_INVALID")]
    [InlineData("org1", "SendOrgRegisterOtpEmail", "test2@gmail.com", "123456", "PROVIDED_OTP_NOTFOUND")]
    [InlineData("org2", "SendOrgRegisterOtpEmail", "test2@gmail.com", "123456", "PROVIDED_OTP_NOTFOUND")]
    [InlineData("org2", "SendOrgRegisterOtpEmail", "test2@gmail.com", "999999", "PROVIDED_OTP_NOTFOUND")]
    [InlineData("org2", "AnotherRegisterOtpEmail", "test2@gmail.com", "123456", "PROVIDED_OTP_NOTFOUND")]
    public void RegisterOrganizationOtpTest(string ordId, string apiKey, string email, string otp, string needStatus)
    {
        //ทดสอบว่าไม่พบ OTP ที่ส่งมา
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "UnitTest");

        var orgSvc = new Mock<IOrganizationService>();
        var userSvc = new Mock<IUserService>();
        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var redisHelper = new Mock<IRedisHelper>();
        var cacheKey = $"{ordId}:UnitTest:{apiKey}:{email}";
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(cacheKey)).ReturnsAsync(new MOtp() { Otp = otp });

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);

        var usrRegister = new MOrganizeRegistration();
        usrRegister.ProofEmailOtp = "999999";
        usrRegister.Email = "test1@gmail.com";
        var result = adminSvc.RegisterOrganization("org1", usrRegister);

        Assert.Equal(needStatus, result.Status);
    }

    [Theory]
    [InlineData("org1", "existing-user", "test1@gmail.com", "USER_ALREADY_EXIST")]
    [InlineData("org1", "new-user", "existing-email@gmail.com", "EMAIL_ALREADY_EXIST")]
    public void RegisterOrganizationUserAndEmailExistTest(string ordId, string userName, string email, string needStatus)
    {
        //ทดสอบว่าไม่พบ OTP ที่ส่งมา
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "UnitTest");
        var otp = "123456";

        var orgSvc = new Mock<IOrganizationService>();

        var userSvc = new Mock<IUserService>();
        userSvc.Setup(s => s.IsUserNameExist(ordId, "existing-user")).Returns(true);
        userSvc.Setup(s => s.IsEmailExist(ordId, "existing-email@gmail.com")).Returns(true);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var redisHelper = new Mock<IRedisHelper>();
        var cacheKey = $"{ordId}:UnitTest:SendOrgRegisterOtpEmail:{email}";
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(cacheKey)).ReturnsAsync(new MOtp() { Otp = otp });

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);

        var usrRegister = new MOrganizeRegistration();
        usrRegister.ProofEmailOtp = otp;
        usrRegister.Email = email;
        usrRegister.UserName = userName;
        var result = adminSvc.RegisterOrganization(ordId, usrRegister);

        Assert.Equal(needStatus, result.Status);
    }

    [Theory]
    [InlineData("existing-org-id", "user1", "test1@gmail.com", "ORG_ALREADY_EXIST")]
    public void RegisterOrganizationOrgIdExistTest(string orgId, string userName, string email, string needStatus)
    {
        //ทดสอบว่าไม่พบ OTP ที่ส่งมา
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "UnitTest");
        var otp = "123456";

        var orgSvc = new Mock<IOrganizationService>();
        orgSvc.Setup(s => s.IsOrgIdExist("existing-org-id")).Returns(true);

        var userSvc = new Mock<IUserService>();
        userSvc.Setup(s => s.IsUserNameExist(orgId, userName)).Returns(false);
        userSvc.Setup(s => s.IsEmailExist(orgId, email)).Returns(false);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var redisHelper = new Mock<IRedisHelper>();
        var cacheKey = $"{orgId}:UnitTest:SendOrgRegisterOtpEmail:{email}";
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(cacheKey)).ReturnsAsync(new MOtp() { Otp = otp });

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);

        var usrRegister = new MOrganizeRegistration();
        usrRegister.ProofEmailOtp = otp;
        usrRegister.Email = email;
        usrRegister.UserName = userName;
        usrRegister.UserOrgId = orgId;
        var result = adminSvc.RegisterOrganization(orgId, usrRegister);

        Assert.Equal(needStatus, result.Status);
    }

    [Theory]
    [InlineData("existing-org-id", "user1", "test1@gmail.com", "ORG_ADD_ERROR")]
    public void RegisterOrganizationAddOrgErrorTest(string orgId, string userName, string email, string needStatus)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "UnitTest");
        var otp = "123456";

        var orgSvc = new Mock<IOrganizationService>();
        orgSvc.Setup(s => s.IsOrgIdExist(orgId)).Returns(false);
        orgSvc.Setup(s => s.AddOrganization(It.IsAny<string>(), It.IsAny<MOrganization>())).Returns(new MVOrganization() { Status = "ORG_ADD_ERROR" });

        var userSvc = new Mock<IUserService>();
        userSvc.Setup(s => s.IsUserNameExist(orgId, userName)).Returns(false);
        userSvc.Setup(s => s.IsEmailExist(orgId, email)).Returns(false);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var redisHelper = new Mock<IRedisHelper>();
        var cacheKey = $"{orgId}:UnitTest:SendOrgRegisterOtpEmail:{email}";
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(cacheKey)).ReturnsAsync(new MOtp() { Otp = otp });

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);

        var usrRegister = new MOrganizeRegistration();
        usrRegister.ProofEmailOtp = otp;
        usrRegister.Email = email;
        usrRegister.UserName = userName;
        usrRegister.UserOrgId = orgId;
        var result = adminSvc.RegisterOrganization(orgId, usrRegister);

        Assert.Equal(needStatus, result.Status);
    }

    [Theory]
    [InlineData("orgid1", "user1", "test1@gmail.com", "USER_ADD_ERROR")]
    public void RegisterOrganizationAddUserErrorTest(string orgId, string userName, string email, string needStatus)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "UnitTest");
        var otp = "123456";

        var orgSvc = new Mock<IOrganizationService>();
        orgSvc.Setup(s => s.IsOrgIdExist(orgId)).Returns(false);
        orgSvc.Setup(s => s.AddOrganization(It.IsAny<string>(), It.IsAny<MOrganization>())).Returns(new MVOrganization() { Status = "OK" });

        var userSvc = new Mock<IUserService>();
        userSvc.Setup(s => s.IsUserNameExist(orgId, userName)).Returns(false);
        userSvc.Setup(s => s.IsEmailExist(orgId, email)).Returns(false);
        userSvc.Setup(s => s.AddUser(It.IsAny<string>(), It.IsAny<MUser>())).Returns(new MVUser() { Status = "USER_ADD_ERROR" });

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var redisHelper = new Mock<IRedisHelper>();
        var cacheKey = $"{orgId}:UnitTest:SendOrgRegisterOtpEmail:{email}";
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(cacheKey)).ReturnsAsync(new MOtp() { Otp = otp });

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);

        var usrRegister = new MOrganizeRegistration();
        usrRegister.ProofEmailOtp = otp;
        usrRegister.Email = email;
        usrRegister.UserName = userName;
        usrRegister.UserOrgId = orgId;
        var result = adminSvc.RegisterOrganization(orgId, usrRegister);

        Assert.Equal(needStatus, result.Status);
    }

    [Theory]
    [InlineData("orgid1", "user1", "test1@gmail.com", "ORG_USER_ADD_ERROR")]
    public void RegisterOrganizationAddOrgUserErrorTest(string orgId, string userName, string email, string needStatus)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "UnitTest");
        var otp = "123456";

        var orgSvc = new Mock<IOrganizationService>();
        orgSvc.Setup(s => s.IsOrgIdExist(orgId)).Returns(false);
        orgSvc.Setup(s => s.AddOrganization(It.IsAny<string>(), It.IsAny<MOrganization>())).Returns(new MVOrganization() { Status = "OK" });
        orgSvc.Setup(s => s.AddUserToOrganization(It.IsAny<string>(), It.IsAny<MOrganizationUser>())).Returns(new MVOrganizationUser() { Status = "ORG_USER_ADD_ERROR" });

        var userSvc = new Mock<IUserService>();
        userSvc.Setup(s => s.IsUserNameExist(orgId, userName)).Returns(false);
        userSvc.Setup(s => s.IsEmailExist(orgId, email)).Returns(false);
        userSvc.Setup(s => s.AddUser(It.IsAny<string>(), It.IsAny<MUser>())).Returns(new MVUser()
        {
            Status = "OK",
            User = new MUser() { UserId = Guid.NewGuid() },
        });

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var redisHelper = new Mock<IRedisHelper>();
        var cacheKey = $"{orgId}:UnitTest:SendOrgRegisterOtpEmail:{email}";
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(cacheKey)).ReturnsAsync(new MOtp() { Otp = otp });

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);

        var usrRegister = new MOrganizeRegistration();
        usrRegister.ProofEmailOtp = otp;
        usrRegister.Email = email;
        usrRegister.UserName = userName;
        usrRegister.UserOrgId = orgId;
        var result = adminSvc.RegisterOrganization(orgId, usrRegister);

        Assert.Equal(needStatus, result.Status);
    }

    [Theory]
    [InlineData("orgid1", "user1", "test1@gmail.com", "IDP_ADD_USER_ERROR")]
    public void RegisterOrganizationAddIdpUserErrorTest(string orgId, string userName, string email, string needStatus)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "UnitTest");
        var otp = "123456";

        var orgSvc = new Mock<IOrganizationService>();
        orgSvc.Setup(s => s.IsOrgIdExist(orgId)).Returns(false);
        orgSvc.Setup(s => s.AddOrganization(It.IsAny<string>(), It.IsAny<MOrganization>())).Returns(new MVOrganization() { Status = "OK" });
        orgSvc.Setup(s => s.AddUserToOrganization(It.IsAny<string>(), It.IsAny<MOrganizationUser>())).Returns(new MVOrganizationUser() { Status = "OK" });

        var userSvc = new Mock<IUserService>();
        userSvc.Setup(s => s.IsUserNameExist(orgId, userName)).Returns(false);
        userSvc.Setup(s => s.IsEmailExist(orgId, email)).Returns(false);
        userSvc.Setup(s => s.AddUser(It.IsAny<string>(), It.IsAny<MUser>())).Returns(new MVUser()
        {
            Status = "OK",
            User = new MUser() { UserId = Guid.NewGuid() },
        });

        var jobSvc = new Mock<IJobService>();

        var authSvc = new Mock<IAuthService>();
        authSvc.Setup(s => s.AddUserToIDP(It.IsAny<MOrganizeRegistration>())).ReturnsAsync(new IdpResult() { Success = false });

        var redisHelper = new Mock<IRedisHelper>();
        var cacheKey = $"{orgId}:UnitTest:SendOrgRegisterOtpEmail:{email}";
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(cacheKey)).ReturnsAsync(new MOtp() { Otp = otp });

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);

        var usrRegister = new MOrganizeRegistration();
        usrRegister.ProofEmailOtp = otp;
        usrRegister.Email = email;
        usrRegister.UserName = userName;
        usrRegister.UserOrgId = orgId;
        var result = adminSvc.RegisterOrganization(orgId, usrRegister);

        Assert.Equal(needStatus, result.Status);
    }

    [Theory]
    [InlineData("orgid1", "user1", "test1@gmail.com", "SUCCESS")]
    public void RegisterOrganizationSuccessTest(string orgId, string userName, string email, string needStatus)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "UnitTest");
        var otp = "123456";

        var orgSvc = new Mock<IOrganizationService>();
        orgSvc.Setup(s => s.IsOrgIdExist(orgId)).Returns(false);
        orgSvc.Setup(s => s.AddOrganization(It.IsAny<string>(), It.IsAny<MOrganization>())).Returns(new MVOrganization() { Status = "OK" });
        orgSvc.Setup(s => s.AddUserToOrganization(It.IsAny<string>(), It.IsAny<MOrganizationUser>())).Returns(new MVOrganizationUser() { Status = "OK" });

        var userSvc = new Mock<IUserService>();
        userSvc.Setup(s => s.IsUserNameExist(orgId, userName)).Returns(false);
        userSvc.Setup(s => s.IsEmailExist(orgId, email)).Returns(false);
        userSvc.Setup(s => s.AddUser(It.IsAny<string>(), It.IsAny<MUser>())).Returns(new MVUser()
        {
            Status = "OK",
            User = new MUser() { UserId = Guid.NewGuid() },
        });

        var jobSvc = new Mock<IJobService>();

        var authSvc = new Mock<IAuthService>();
        authSvc.Setup(s => s.AddUserToIDP(It.IsAny<MOrganizeRegistration>())).ReturnsAsync(new IdpResult() { Success = true });

        var redisHelper = new Mock<IRedisHelper>();
        var cacheKey = $"{orgId}:UnitTest:SendOrgRegisterOtpEmail:{email}";
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(cacheKey)).ReturnsAsync(new MOtp() { Otp = otp });

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);

        var usrRegister = new MOrganizeRegistration();
        usrRegister.ProofEmailOtp = otp;
        usrRegister.Email = email;
        usrRegister.UserName = userName;
        usrRegister.UserOrgId = orgId;
        var result = adminSvc.RegisterOrganization(orgId, usrRegister);

        Assert.Equal(needStatus, result.Status);
    }


    [Theory]
    [InlineData("orgid1", "user1", "test1@gmail.com", "SUCCESS")]
    public void RegisterOrganizationRetunNullTest(string orgId, string userName, string email, string needStatus)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "UnitTest");
        var otp = "123456";

        var orgSvc = new Mock<IOrganizationService>();
        orgSvc.Setup(s => s.IsOrgIdExist(orgId)).Returns(false);
        orgSvc.Setup(s => s.AddOrganization(It.IsAny<string>(), It.IsAny<MOrganization>())).Returns(new MVOrganization() { Status = "OK" });
        orgSvc.Setup(s => s.AddUserToOrganization(It.IsAny<string>(), It.IsAny<MOrganizationUser>())).Returns(new MVOrganizationUser() { Status = "OK" });

        var userSvc = new Mock<IUserService>();
        userSvc.Setup(s => s.IsUserNameExist(orgId, userName)).Returns(false);
        userSvc.Setup(s => s.IsEmailExist(orgId, email)).Returns(false);
        userSvc.Setup(s => s.AddUser(It.IsAny<string>(), It.IsAny<MUser>())).Returns(new MVUser()
        {
            Status = "OK",
            User = new MUser() { UserId = Guid.NewGuid() },
        });

        var jobSvc = new Mock<IJobService>();

        var authSvc = new Mock<IAuthService>();
        authSvc.Setup(s => s.AddUserToIDP(It.IsAny<MOrganizeRegistration>())).ReturnsAsync(new IdpResult() { Success = true });

        var redisHelper = new Mock<IRedisHelper>();
        var cacheKey = $"{orgId}:UnitTest:SendOrgRegisterOtpEmail:{email}";
        redisHelper.Setup(s => s.GetObjectAsync<MOtp>(cacheKey)).ReturnsAsync(new MOtp() { Otp = otp });

        var adminSvc = new AdminService(orgSvc.Object, userSvc.Object, jobSvc.Object, authSvc.Object, redisHelper.Object);

        var usrRegister = new MOrganizeRegistration();
        usrRegister.ProofEmailOtp = otp;
        usrRegister.Email = email;
        usrRegister.UserName = userName;
        usrRegister.UserOrgId = orgId;
        var result = adminSvc.RegisterOrganization(orgId, usrRegister);

        Assert.Equal(needStatus, result.Status);
    }
}