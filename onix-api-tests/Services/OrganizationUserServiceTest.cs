using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Test.Services;

public class OrganizationUserServiceTest
{
    //===== GetUsers() =====
    [Theory]
    [InlineData("org1")]
    [InlineData("org2")]
    public void GetUsersTest(string orgId)
    {
        var param = new VMOrganizationUser() { };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.GetUsers(param)).Returns([]);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.GetUsers(orgId, param);

        Assert.Empty(result.ToArray());
    }
    //=====

    //===== GetUsersLeftJoin() =====
    [Theory]
    [InlineData("org1")]
    [InlineData("org2")]
    public void GetUsersLeftJoinTest(string orgId)
    {
        var param = new VMOrganizationUser() { };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.GetUsersLeftJoin(param)).Returns([]);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.GetUsersLeftJoin(orgId, param);

        Assert.Empty(result.ToArray());
    }
    //=====

    //===== AddUser() =====
    [Theory]
    [InlineData("org1")]
    [InlineData("org2")]
    public void AddUserOkTest(string orgId)
    {
        var user = new MOrganizationUser() { UserName = "testuser", UserEmail = "test@email.com" };
        var u = new MUser() { UserName = user.UserName, UserEmail = user.UserEmail, UserId = Guid.NewGuid() };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.AddUser(user)).Returns(user);

        var jobSvc = new Mock<IJobService>();

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(s => s.AddUser(It.IsAny<MUser>())).Returns(u);

        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.AddUser(orgId, user);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //=====

    //===== IdentifyRegistrationCase() =====
    [Theory]
    [InlineData("test@email.com", "aaaa@email.com")]
    public void IdentifyRegistrationCaseUserNameIsUsedTest(string ouEmail, string uEmail)
    {
        var ou = new MOrganizationUser() { UserName = "testuser", TmpUserEmail = ouEmail };
        var u = new MUser() { UserName = ou.UserName, UserEmail = uEmail, UserId = Guid.NewGuid() };

        var repo = new Mock<IOrganizationUserRepository>();
        var jobSvc = new Mock<IJobService>();

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(s => s.GetUserByName(ou.UserName)).Returns(u);

        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.IdentifyRegistrationCase(ou);

        Assert.NotNull(result);
        Assert.Equal("ERROR_NAME_IS_USED_BY_ANOTHER", result);
    }

    [Theory]
    [InlineData("test@email.com", "test@email.com")]
    public void IdentifyRegistrationCaseUserIsRegisteredTest(string ouEmail, string uEmail)
    {
        var ou = new MOrganizationUser() { UserName = "testuser", TmpUserEmail = ouEmail };
        var u = new MUser() { UserName = ou.UserName, UserEmail = uEmail, UserId = Guid.NewGuid() };

        var repo = new Mock<IOrganizationUserRepository>();
        var jobSvc = new Mock<IJobService>();

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(s => s.GetUserByName(ou.UserName)).Returns(u);

        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.IdentifyRegistrationCase(ou);

        Assert.NotNull(result);
        Assert.Equal("OK_TO_ADD_IN_ORG1", result);
    }

    [Theory]
    [InlineData("test@email.com")]
    public void IdentifyRegistrationCaseUserHasNotRegisteredTest(string ouEmail)
    {
        var ou = new MOrganizationUser() { UserName = "testuser", TmpUserEmail = ouEmail };
        MUser u = null!;

        var repo = new Mock<IOrganizationUserRepository>();
        var jobSvc = new Mock<IJobService>();

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(s => s.GetUserByName(ou.UserName)).Returns(u);
        userRepo.Setup(s => s.GetUserByEmail(ou.TmpUserEmail)).Returns(u);

        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.IdentifyRegistrationCase(ou);

        Assert.NotNull(result);
        Assert.Equal("OK_TO_ADD_IN_ORG2", result);
    }

    [Theory]
    [InlineData("test1@email.com", "test2@email.com")]
    public void IdentifyRegistrationCaseEmailIsUsedByAnotherTest(string ouEmail, string uEmail)
    {
        var ou = new MOrganizationUser() { UserName = "testuser", TmpUserEmail = ouEmail };
        MUser u1 = null!;
        MUser u2 = new MUser() { UserName = ou.UserName, UserEmail = uEmail, UserId = Guid.NewGuid() };

        var repo = new Mock<IOrganizationUserRepository>();
        var jobSvc = new Mock<IJobService>();

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(s => s.GetUserByName(ou.UserName)).Returns(u1);
        userRepo.Setup(s => s.GetUserByEmail(ou.TmpUserEmail)).Returns(u2);

        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.IdentifyRegistrationCase(ou);

        Assert.NotNull(result);
        Assert.Equal("ERROR_EMAIL_IS_USED_BY_ANOTHER", result);
    }
    //=====

    //===== DeleteUserById() =====
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeex")]
    public void DeleteUserByIdInvalidIdTest(string orgId, string uid)
    {
        var repo = new Mock<IOrganizationUserRepository>();
        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.DeleteUserById(orgId, uid);

        Assert.NotNull(result);
        Assert.Equal("UUID_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    public void DeleteUserByIdUserNotFoundTest(string orgId, string uid)
    {
        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.GetUserById(uid)).ReturnsAsync((MOrganizationUser)null!);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.DeleteUserById(orgId, uid);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND_GET_USER", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    public void DeleteUserByIdUserIsOrgInitialTest(string orgId, string uid)
    {
        var ou = new MOrganizationUser() { IsOrgInitialUser = "YES" };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.GetUserByIdLeftJoin(uid)).ReturnsAsync(ou);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.DeleteUserById(orgId, uid);

        Assert.NotNull(result);
        Assert.Equal("NOT_ALLOW_DELETE_INITIAL_USER", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    public void DeleteUserByIdUserNotFound2Test(string orgId, string uid)
    {
        var ou1 = new MOrganizationUser() { IsOrgInitialUser = "NO" };
        MOrganizationUser ou2 = null!;

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.GetUserByIdLeftJoin(uid)).ReturnsAsync(ou1);
        repo.Setup(s => s.DeleteUserById(uid)).Returns(ou2);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.DeleteUserById(orgId, uid);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND_DELTE_USER", result.Status);
    }
    //=====

    //===== GetUserById() =====
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", "OWNER")]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", "OWNER,VIEWER")]
    public void GetUserByIdOkTest(string orgId, string uid, string roleList)
    {
        var ou1 = new MOrganizationUser() { RolesList = roleList };
        var expectedRoleCount = roleList.Split(',').Length;

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.GetUserById(uid)).ReturnsAsync(ou1);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.GetUserById(orgId, uid);

        Assert.NotNull(result);
        Assert.Equal("", result.RolesList);
        Assert.Equal(expectedRoleCount, result.Roles.Count);
    }
    //=====

    //===== GetUserByIdLeftJoin() =====
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeex", "OWNER")]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeey", "OWNER,VIEWER")]
    public void GetUserByIdLeftJoinInvalidIdTest(string orgId, string uid, string roleList)
    {
        var ou1 = new MOrganizationUser() { RolesList = roleList };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.GetUserByIdLeftJoin(uid)).ReturnsAsync(ou1);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.GetUserByIdLeftJoin(orgId, uid);

        Assert.NotNull(result);
        Assert.Equal("UUID_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeea")]
    public void GetUserByIdLeftJoinUserNotFoundTest(string orgId, string uid)
    {
        MOrganizationUser ou1 = null!; //new MOrganizationUser() { RolesList = roleList };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.GetUserByIdLeftJoin(uid)).ReturnsAsync(ou1);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.GetUserByIdLeftJoin(orgId, uid);

        Assert.NotNull(result);
        Assert.Equal("USER_ID_NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", "OWNER")]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeea", "OWNER,VIEWER")]
    public void GetUserByIdLeftJoinOkTest(string orgId, string uid, string roleList)
    {
        var ou1 = new MOrganizationUser() { RolesList = roleList };
        var expectedRoleCount = roleList.Split(',').Length;

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.GetUserByIdLeftJoin(uid)).ReturnsAsync(ou1);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.GetUserByIdLeftJoin(orgId, uid);

        Assert.NotNull(result);
        Assert.NotNull(result.OrgUser);
        Assert.Equal("", result.OrgUser.RolesList);
        Assert.Equal(expectedRoleCount, result.OrgUser.Roles.Count);
    }
    //=====

    //===== GetUserCount() =====
    [Theory]
    [InlineData("org1", 5)]
    [InlineData("org1", 4)]
    public void GetUserCountOkTest(string orgId, int expectedCount)
    {
        var param = new VMOrganizationUser() { };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.GetUserCount(param)).Returns(expectedCount);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.GetUserCount(orgId, param);

        Assert.Equal(expectedCount, result);
    }
    //=====

    //===== GetUserCountLeftJoin() =====
    [Theory]
    [InlineData("org1", 5)]
    [InlineData("org1", 4)]
    public void GetUserCountLeftJoinOkTest(string orgId, int expectedCount)
    {
        var param = new VMOrganizationUser() { };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.GetUserCountLeftJoin(param)).Returns(expectedCount);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.GetUserCountLeftJoin(orgId, param);

        Assert.Equal(expectedCount, result);
    }
    //=====

    //===== UpdateUserById() =====
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeex")]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeey")]
    public void UpdateUserByIdInvalidIdTest(string orgId, string uid)
    {
        var ou1 = new MOrganizationUser() { RolesList = "" };

        var repo = new Mock<IOrganizationUserRepository>();
        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.UpdateUserById(orgId, uid, ou1);

        Assert.NotNull(result);
        Assert.Equal("UUID_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", "")]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeea", "OWNER,VIEWER")]
    public void UpdateUserByIdUserOkTest(string orgId, string uid, string roleList)
    {
        List<string> roles = null!;
        if (!string.IsNullOrEmpty(roleList))
        {
            roles = roleList.Split(',').ToList();
        }

        var ou1 = new MOrganizationUser() { Roles = roles };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.UpdateUserById(uid, ou1)).Returns(ou1);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.UpdateUserById(orgId, uid, ou1);

        Assert.NotNull(result);
        Assert.NotNull(result.OrgUser);

        Assert.Equal("OK", result.Status);
        Assert.Equal(roleList, result.OrgUser.RolesList);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", "")]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeea", "OWNER,VIEWER")]
    public void UpdateUserByIdUserNotFoundTest(string orgId, string uid, string roleList)
    {
        List<string> roles = null!;
        if (!string.IsNullOrEmpty(roleList))
        {
            roles = roleList.Split(',').ToList();
        }

        var ou1 = new MOrganizationUser() { Roles = roles };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.UpdateUserById(uid, ou1)).Returns((MOrganizationUser)null!);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.UpdateUserById(orgId, uid, ou1);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }
    //=====

    //===== InviteUser() =====
    [Theory]
    [InlineData("org1", "")]
    [InlineData("org2", "")]
    public void InviteUserUserNameInvalidTest(string orgId, string userName)
    {
        var ou1 = new MOrganizationUser() { UserName = userName };

        var repo = new Mock<IOrganizationUserRepository>();

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.InviteUser(orgId, ou1);

        Assert.NotNull(result);
        Assert.Equal("ERROR_VALIDATION_USERNAME", result.Status);
    }

    [Theory]
    [InlineData("org1", "user1")]
    [InlineData("org2", "user2")]
    public void InviteUserUserNameDuplicateTest(string orgId, string userName)
    {
        var ou1 = new MOrganizationUser() { UserName = userName };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.IsUserNameExist(userName)).Returns(true);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.InviteUser(orgId, ou1);

        Assert.NotNull(result);
        Assert.Equal("USERNAME_DUPLICATE", result.Status);
    }

    [Theory]
    [InlineData("org1", "user1")]
    [InlineData("org2", "user2")]
    public void InviteUserEmailEmptyTest(string orgId, string userName)
    {
        var ou1 = new MOrganizationUser() { UserName = userName, TmpUserEmail = "" };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.IsUserNameExist(userName)).Returns(false);

        var jobSvc = new Mock<IJobService>();
        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.InviteUser(orgId, ou1);

        Assert.NotNull(result);
        Assert.Equal("INVALID_EMAIL_EMPTY", result.Status);
    }


    [Theory]
    [InlineData("org1", "user1")]
    [InlineData("org2", "user2")]
    public void InviteUserEmailInvalidTest(string orgId, string userName)
    {
        var ou1 = new MOrganizationUser() { UserName = userName, TmpUserEmail = "invalid-email.com" };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.IsUserNameExist(userName)).Returns(false);

        var jobSvc = new Mock<IJobService>();

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.InviteUser(orgId, ou1);

        Assert.NotNull(result);
        Assert.Equal("ERROR_VALIDATION_EMAIL", result.Status);
    }

    [Theory]
    [InlineData("org1", "user1")]
    [InlineData("org2", "user2")]
    public void InviteUserRegisterErrorTest(string orgId, string userName)
    {
        var ou1 = new MOrganizationUser() { UserName = userName, TmpUserEmail = "test1@valid-email.com" };

        MUser u1 = null!;
        MUser u2 = new MUser() { UserName = ou1.UserName, UserEmail = "test2@valid-email.com", UserId = Guid.NewGuid() };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.IsUserNameExist(userName)).Returns(false);

        var jobSvc = new Mock<IJobService>();

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(s => s.GetUserByName(ou1.UserName)).Returns(u1);
        userRepo.Setup(s => s.GetUserByEmail(ou1.TmpUserEmail)).Returns(u2);

        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.InviteUser(orgId, ou1);

        Assert.NotNull(result);
        Assert.Equal("ERROR_EMAIL_IS_USED_BY_ANOTHER", result.Status);
    }
    
    [Theory]
    [InlineData("org1", "user1", "")]
    [InlineData("org2", "user2", "OWNER,VIEWER")]
    public void InviteUserRegisterOkTest(string orgId, string userName, string roleList)
    {
        List<string> roles = null!;
        if (!string.IsNullOrEmpty(roleList))
        {
            roles = roleList.Split(',').ToList();
        }

        var ou1 = new MOrganizationUser()
        {
            UserName = userName,
            TmpUserEmail = "test1@valid-email.com",
            Roles = roles,
        };
        
        MUser u1 = new MUser() { UserName = ou1.UserName, UserEmail = ou1.TmpUserEmail, UserId = Guid.NewGuid() };

        var repo = new Mock<IOrganizationUserRepository>();
        repo.Setup(s => s.IsUserNameExist(userName)).Returns(false);
        repo.Setup(s => s.AddUser(ou1)).Returns(ou1);

        var jobSvc = new Mock<IJobService>();

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(s => s.GetUserByName(ou1.UserName)).Returns(u1);

        var redisHelper = new Mock<IRedisHelper>();

        var userSvc = new OrganizationUserService(repo.Object, userRepo.Object, jobSvc.Object, redisHelper.Object);
        var result = userSvc.InviteUser(orgId, ou1);

        Assert.NotNull(result);
        Assert.NotNull(result.OrgUser);

        Assert.Equal("OK", result.Status);
        Assert.Equal("", result.OrgUser.RolesList);
    }
    //=====
}
