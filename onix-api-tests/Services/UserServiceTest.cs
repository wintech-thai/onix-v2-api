using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Test.Services;

public class UserServiceTest
{
    //===== UpdatePassword() ====
    [Theory]
    [InlineData("user1", "thi12X@newpassword")] //ยาวไป
    [InlineData("user1", "t2!A")] //สั้นไป
    [InlineData("user1", "t2AbcdeFGH")] //ไม่มีอักขระพิเศษ
    [InlineData("user1", "t@AbcdeFGH")] //ไม่มีตัวเลข
    [InlineData("user1", "t@abcdefgh")] //ไม่มีตัวใหญ๋
    [InlineData("user1", "T@ABCDEFGH")] //ไม่มีตัวเล็ก
    public void UpdatePasswordValidationFailTest(string userName, string password)
    {
        var passwdObj = new MUpdatePassword() { NewPassword = password };

        var repo = new Mock<IUserRepository>();
        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.UpdatePassword(userName, passwdObj);

        Assert.NotNull(result);
        Assert.Equal("ERROR_VALIDATION_PASSWORD", result.Status);
    }

    [Theory]
    [InlineData("user1", "thi12X@newPasrd")] //แบบนี้ validation OK
    [InlineData("user1", "$#A12X@newPasrd")] //แบบนี้ validation OK
    public void UpdatePasswordIdpErrorTest(string userName, string password)
    {
        var passwdObj = new MUpdatePassword() { NewPassword = password };
        var idpResult = new IdpResult() { Success = false };

        var repo = new Mock<IUserRepository>();
        var jobSvc = new Mock<IJobService>();

        var authSvc = new Mock<IAuthService>();
        authSvc.Setup(s => s.ChangeUserPasswordIdp(passwdObj)).ReturnsAsync(idpResult);

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.UpdatePassword(userName, passwdObj);

        Assert.NotNull(result);
        Assert.Equal("IDP_UPDATE_PASSWORD_ERROR", result.Status);
    }

    private MUser CreateMockedNullUser()
    {
        return null!;
    }

    [Theory]
    [InlineData("user1", "thi12X@newPasrd")] //แบบนี้ validation OK
    [InlineData("user1", "$#A12X@newPasrd")] //แบบนี้ validation OK
    public void UpdatePasswordUserNameNotFoundTest(string userName, string password)
    {
        var passwdObj = new MUpdatePassword() { NewPassword = password };
        var idpResult = new IdpResult() { Success = true };

        var repo = new Mock<IUserRepository>();
        repo.Setup(s => s.GetUserByName(userName)).Returns(CreateMockedNullUser());

        var jobSvc = new Mock<IJobService>();

        var authSvc = new Mock<IAuthService>();
        authSvc.Setup(s => s.ChangeUserPasswordIdp(passwdObj)).ReturnsAsync(idpResult);

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.UpdatePassword(userName, passwdObj);

        Assert.NotNull(result);
        Assert.Equal("USERNAME_NOT_FOUND_DB", result.Status);
    }

    [Theory]
    [InlineData("user1", "thi12X@newPasrd")] //แบบนี้ validation OK
    [InlineData("user1", "$#A12X@newPasrd")] //แบบนี้ validation OK
    public void UpdatePasswordOkTest(string userName, string password)
    {
        var passwdObj = new MUpdatePassword() { NewPassword = password };
        var idpResult = new IdpResult() { Success = true };

        var repo = new Mock<IUserRepository>();
        repo.Setup(s => s.GetUserByName(userName)).Returns(new MUser() { });

        var jobSvc = new Mock<IJobService>();

        var authSvc = new Mock<IAuthService>();
        authSvc.Setup(s => s.ChangeUserPasswordIdp(passwdObj)).ReturnsAsync(idpResult);

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.UpdatePassword(userName, passwdObj);

        Assert.NotNull(result);
        Assert.Equal("SUCCESS", result.Status);
    }
    //=====

    //===== UserLogout() =====
    [Theory]
    [InlineData("user1")]
    [InlineData("user2")]
    public void UserLogoutIdpErrorTest(string userName)
    {
        var idpResult = new IdpResult() { Success = false };

        var repo = new Mock<IUserRepository>();
        var jobSvc = new Mock<IJobService>();

        var authSvc = new Mock<IAuthService>();
        authSvc.Setup(s => s.UserLogoutIdp(userName)).ReturnsAsync(idpResult);

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.UserLogout(userName);

        Assert.NotNull(result);
        Assert.Equal("IDP_LOGOUT_ERROR", result.Status);
    }

    [Theory]
    [InlineData("user1")]
    [InlineData("user2")]
    public void UserLogoutIdpOkTest(string userName)
    {
        var idpResult = new IdpResult() { Success = true };

        var repo = new Mock<IUserRepository>();
        var jobSvc = new Mock<IJobService>();

        var authSvc = new Mock<IAuthService>();
        authSvc.Setup(s => s.UserLogoutIdp(userName)).ReturnsAsync(idpResult);

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.UserLogout(userName);

        Assert.NotNull(result);
        Assert.Equal("SUCCESS", result.Status);
    }
    //=====

    //===== GetUserByEmail() =====
    [Theory]
    [InlineData("org1", "email1")]
    [InlineData("org2", "email2")]
    public void GetUserByEmailNotFoundTest(string orgId, string email)
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(s => s.GetUserByEmail(email)).Returns((MUser)null!);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.GetUserByEmail(orgId, email);

        Assert.Null(result);
    }
    
    [Theory]
    [InlineData("org1", "email1")]
    [InlineData("org2", "email2")]
    public void GetUserByEmailFoundTest(string orgId, string email)
    {
        var u = new MUser()
        {
        };

        var repo = new Mock<IUserRepository>();
        repo.Setup(s => s.GetUserByEmail(email)).Returns(u);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.GetUserByEmail(orgId, email);

        Assert.NotNull(result);
    }
    //=====

    //===== IsUserIdExist() =====
    [Theory]
    [InlineData("org1", "user1", true)]
    [InlineData("org2", "user2", false)]
    public void IsUserIdExistTest(string orgId, string userId, bool userExist)
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(s => s.IsUserIdExist(userId)).Returns(userExist);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.IsUserIdExist(orgId, userId);

        Assert.Equal(userExist, result);
    }
    //=====

    //===== IsUserNameExist() =====
    [Theory]
    [InlineData("org1", "user1", true)]
    [InlineData("org2", "user2", false)]
    public void IsUserNameExistTest(string orgId, string userName, bool userExist)
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(s => s.IsUserNameExist(userName)).Returns(userExist);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.IsUserNameExist(orgId, userName);

        Assert.Equal(userExist, result);
    }
    //=====

    //===== IsEmailExist() =====
    [Theory]
    [InlineData("org1", "user1", true)]
    [InlineData("org2", "user2", false)]
    public void IsEmailExistTest(string orgId, string email, bool userExist)
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(s => s.IsEmailExist(email)).Returns(userExist);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.IsEmailExist(orgId, email);

        Assert.Equal(userExist, result);
    }
    //=====

    //===== GetUsers() =====
    [Theory]
    [InlineData("org1")]
    [InlineData("org2")]
    public void GetUsersTest(string orgId)
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(s => s.GetUsers()).Returns([]);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.GetUsers(orgId);

        Assert.Empty(result.ToArray());
    }
    //=====

    //===== AddUser() =====
    [Theory]
    [InlineData("org1", "email1@gmail.com", "user1ssssssssssssssss")] //ยาวไป
    [InlineData("org2", "email2@gmail.com", "us")] //สั้นไป
    public void AddUserUserNameValidationTest(string orgId, string email, string userName)
    {
        var user = new MUser() { UserEmail = email, UserName = userName };

        var repo = new Mock<IUserRepository>();
        repo.Setup(s => s.IsEmailExist(email)).Returns(true);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.AddUser(orgId, user);

        Assert.Equal("ERROR_VALIDATION_USERNAME", result.Status);
    }

    [Theory]
    [InlineData("org1", "email1@gmail", "pjame.naja")]
    [InlineData("org2", "@gmail.com", "test_sample")]
    [InlineData("org2", "@.com", "test_sample")]
    [InlineData("org2", ".com", "test_sample")]
    public void AddUserEmailValidationTest(string orgId, string email, string userName)
    {
        var user = new MUser() { UserEmail = email, UserName = userName };

        var repo = new Mock<IUserRepository>();
        repo.Setup(s => s.IsEmailExist(email)).Returns(true);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.AddUser(orgId, user);

        Assert.Equal("ERROR_VALIDATION_EMAIL", result.Status);
    }

    [Theory]
    [InlineData("org1", "email1@gmail.com", "user1")]
    [InlineData("org2", "email2@gmail.com", "user2")]
    public void AddUserEmailDuplicateTest(string orgId, string email, string userName)
    {
        var user = new MUser() { UserEmail = email, UserName = userName };

        var repo = new Mock<IUserRepository>();
        repo.Setup(s => s.IsEmailExist(email)).Returns(true);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.AddUser(orgId, user);

        Assert.Equal("EMAIL_DUPLICATE", result.Status);
    }

    [Theory]
    [InlineData("org1", "email1@gmail.com", "user1")]
    [InlineData("org2", "email2@gmail.com", "user2")]
    public void AddUserUserNameDuplicateTest(string orgId, string email, string userName)
    {
        var user = new MUser() { UserName = userName, UserEmail = email };

        var repo = new Mock<IUserRepository>();
        repo.Setup(s => s.IsEmailExist(email)).Returns(false);
        repo.Setup(s => s.IsUserNameExist(userName)).Returns(true);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.AddUser(orgId, user);

        Assert.Equal("USERNAME_DUPLICATE", result.Status);
    }

    [Theory]
    [InlineData("org1", "email1@gmail.com", "user1")]
    [InlineData("org2", "email2@gmail.com", "user2")]
    public void AddUserUserOkTest(string orgId, string email, string userName)
    {
        var user = new MUser() { UserName = userName, UserEmail = email };

        var repo = new Mock<IUserRepository>();
        repo.Setup(s => s.IsEmailExist(email)).Returns(false);
        repo.Setup(s => s.IsUserNameExist(userName)).Returns(false);

        var jobSvc = new Mock<IJobService>();
        var authSvc = new Mock<IAuthService>();

        var userSvc = new UserService(repo.Object, jobSvc.Object, authSvc.Object);
        var result = userSvc.AddUser(orgId, user);

        Assert.Equal("OK", result.Status);
    }
    //=====
}