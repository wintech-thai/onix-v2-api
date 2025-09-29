using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Its.Onix.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.ModelsViews;
using System.Text.Json;

namespace Its.Onix.Api.Test.Controllers;

public class OnlyUserControllerTest
{
    [Fact]
    public void GetUserAllowedOrgNoIdentityTypeTest()
    {
        IUserService service = Mock.Of<IUserService>();
        IOrganizationService orgService = Mock.Of<IOrganizationService>();

        var uc = new OnlyUserController(service, orgService);
        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var t = uc.GetUserAllowedOrg();

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(t);
        Assert.Contains("Unable to identify identity type", badRequestResult.Value!.ToString());
    }

    [Theory]
    [InlineData("API-KEY")]
    [InlineData("XXX")]
    public void GetUserAllowedOrgNoJwtTest(string authType)
    {
        IOrganizationService orgService = Mock.Of<IOrganizationService>();
        IUserService service = Mock.Of<IUserService>();
        var uc = new OnlyUserController(service, orgService);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();
        var response = uc.ControllerContext.HttpContext.Response;
        response.HttpContext.Items["Temp-Identity-Type"] = authType;

        var t = uc.GetUserAllowedOrg();

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(t);
        Assert.Contains("Only allow for JWT identity type", badRequestResult.Value!.ToString());
    }

    [Theory]
    [InlineData("JWT")]
    public void GetUserAllowedOrgNoUserNameTest(string authType)
    {
        IOrganizationService orgService = Mock.Of<IOrganizationService>();
        IUserService service = Mock.Of<IUserService>();
        var uc = new OnlyUserController(service, orgService);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var response = uc.ControllerContext.HttpContext.Response;
        response.HttpContext.Items["Temp-Identity-Type"] = authType;

        var t = uc.GetUserAllowedOrg();

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(t);
        Assert.Contains("Unable to find user name", badRequestResult.Value!.ToString());
    }

    [Theory]
    [InlineData("JWT")]
    public void GetUserAllowedOrgUserNameEmptyTest(string authType)
    {
        IOrganizationService orgService = Mock.Of<IOrganizationService>();
        IUserService service = Mock.Of<IUserService>();
        var uc = new OnlyUserController(service, orgService);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var response = uc.ControllerContext.HttpContext.Response;
        response.HttpContext.Items["Temp-Identity-Type"] = authType;
        response.HttpContext.Items["Temp-Identity-Name"] = "";

        var t = uc.GetUserAllowedOrg();

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(t);
        Assert.Contains("User name is empty", badRequestResult.Value!.ToString());
    }

    [Theory]
    [InlineData("JWT", "user1")]
    [InlineData("JWT", "user2")]
    public void GetUserAllowedOrgOkTest(string authType, string jwtUser)
    {
        var orgService = new Mock<IOrganizationService>();
        orgService.Setup(s => s.GetUserAllowedOrganization(jwtUser)).Returns(new List<MOrganizationUser>
        {
            new MOrganizationUser { OrgCustomId = "xxxx" },
            new MOrganizationUser { OrgCustomId = "yyyy" }
        });

        var service = new Mock<IUserService>();

        var uc = new OnlyUserController(service.Object, orgService.Object);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var response = uc.ControllerContext.HttpContext.Response;
        response.HttpContext.Items["Temp-Identity-Type"] = authType;
        response.HttpContext.Items["Temp-Identity-Name"] = jwtUser;

        var t = uc.GetUserAllowedOrg();

        Assert.IsType<OkObjectResult>(t);
    }

    [Theory]
    [InlineData("JWT", "user1")]
    public void LogoutOkTest(string authType, string jwtUser)
    {
        IOrganizationService orgService = Mock.Of<IOrganizationService>();
        var service = new Mock<IUserService>();
        service.Setup(s => s.UserLogout(jwtUser)).Returns(new MVLogout()
        {
            Status = "SUCCESS",
        });

        var uc = new OnlyUserController(service.Object, orgService);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var response = uc.ControllerContext.HttpContext.Response;
        response.HttpContext.Items["Temp-Identity-Type"] = authType;
        response.HttpContext.Items["Temp-Identity-Name"] = jwtUser;

        var t = uc.Logout("temp");

        Assert.IsType<OkObjectResult>(t);
    }

    //==================================================================

    [Theory]
    [InlineData("org1")]
    [InlineData("org2")]
    public void LogoutNoIdentityTypeTest(string orgId)
    {
        IUserService service = Mock.Of<IUserService>();
        IOrganizationService orgService = Mock.Of<IOrganizationService>();

        var uc = new OnlyUserController(service, orgService);
        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var t = uc.Logout(orgId);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(t);
        Assert.Contains("Unable to identify identity type", badRequestResult.Value!.ToString());
    }

    [Theory]
    [InlineData("API-KEY")]
    [InlineData("XXX")]
    public void LogoutNoJwtTest(string authType)
    {
        IOrganizationService orgService = Mock.Of<IOrganizationService>();
        IUserService service = Mock.Of<IUserService>();
        var uc = new OnlyUserController(service, orgService);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var response = uc.ControllerContext.HttpContext.Response;
        response.HttpContext.Items["Temp-Identity-Type"] = authType;

        var request = new MUpdatePassword();

        var t = uc.Logout("temp");

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(t);
        Assert.Contains("Only allow for JWT identity type", badRequestResult.Value!.ToString());
    }

    [Theory]
    [InlineData("JWT")]
    public void LogoutNoUserNameTest(string authType)
    {
        IOrganizationService orgService = Mock.Of<IOrganizationService>();
        IUserService service = Mock.Of<IUserService>();
        var uc = new OnlyUserController(service, orgService);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var response = uc.ControllerContext.HttpContext.Response;
        response.HttpContext.Items["Temp-Identity-Type"] = authType;

        var t = uc.Logout("temp");

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(t);
        Assert.Contains("Unable to find user name", badRequestResult.Value!.ToString());
    }

    [Theory]
    [InlineData("JWT")]
    public void LogoutUserNameEmptyTest(string authType)
    {
        IOrganizationService orgService = Mock.Of<IOrganizationService>();
        IUserService service = Mock.Of<IUserService>();
        var uc = new OnlyUserController(service, orgService);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var response = uc.ControllerContext.HttpContext.Response;
        response.HttpContext.Items["Temp-Identity-Type"] = authType;
        response.HttpContext.Items["Temp-Identity-Name"] = "";

        var t = uc.Logout("temp");

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(t);
        Assert.Contains("User name is empty", badRequestResult.Value!.ToString());
    }

    //==================================================================

    [Theory]
    [InlineData("org1")]
    [InlineData("org2")]
    public void UpdatePasswordNoIdentityTypeTest(string orgId)
    {
        IUserService service = Mock.Of<IUserService>();
        IOrganizationService orgService = Mock.Of<IOrganizationService>();

        var uc = new OnlyUserController(service, orgService);
        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var request = new MUpdatePassword();

        var t = uc.UpdatePassword(orgId, request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(t);
        Assert.Contains("Unable to identify identity type", badRequestResult.Value!.ToString());
    }

    [Theory]
    [InlineData("API-KEY")]
    [InlineData("XXX")]
    public void UpdatePasswordNoJwtTest(string authType)
    {
        IOrganizationService orgService = Mock.Of<IOrganizationService>();
        IUserService service = Mock.Of<IUserService>();
        var uc = new OnlyUserController(service, orgService);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var response = uc.ControllerContext.HttpContext.Response;
        response.HttpContext.Items["Temp-Identity-Type"] = authType;

        var request = new MUpdatePassword();

        var t = uc.UpdatePassword("temp", request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(t);
        Assert.Contains("Only allow for JWT identity type", badRequestResult.Value!.ToString());
    }

    [Theory]
    [InlineData("JWT")]
    public void UpdatePasswordNoUserNameTest(string authType)
    {
        IOrganizationService orgService = Mock.Of<IOrganizationService>();
        IUserService service = Mock.Of<IUserService>();
        var uc = new OnlyUserController(service, orgService);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var response = uc.ControllerContext.HttpContext.Response;
        response.HttpContext.Items["Temp-Identity-Type"] = authType;

        var request = new MUpdatePassword();

        var t = uc.UpdatePassword("temp", request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(t);
        Assert.Contains("Unable to find user name", badRequestResult.Value!.ToString());
    }

    [Theory]
    [InlineData("JWT")]
    public void UpdatePasswordUserNameEmptyTest(string authType)
    {
        IOrganizationService orgService = Mock.Of<IOrganizationService>();
        IUserService service = Mock.Of<IUserService>();
        var uc = new OnlyUserController(service, orgService);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var response = uc.ControllerContext.HttpContext.Response;
        response.HttpContext.Items["Temp-Identity-Type"] = authType;
        response.HttpContext.Items["Temp-Identity-Name"] = "";

        var request = new MUpdatePassword();

        var t = uc.UpdatePassword("temp", request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(t);
        Assert.Contains("User name is empty", badRequestResult.Value!.ToString());
    }

    [Theory]
    [InlineData("JWT", "user1", "user1")]
    [InlineData("JWT", "user1", "user2")]
    [InlineData("JWT", "user1", null)]
    public void UpdatePasswordOkTest(string authType, string jwtUser, string? injectedUser)
    {
        IOrganizationService orgService = Mock.Of<IOrganizationService>();
        var service = new Mock<IUserService>();
        service.Setup(s => s.UpdatePassword(It.IsAny<string>(), It.IsAny<MUpdatePassword>())).Returns(new MVUpdatePassword()
        {
            Status = "SUCCESS",
        });

        var uc = new OnlyUserController(service.Object, orgService);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var response = uc.ControllerContext.HttpContext.Response;
        response.HttpContext.Items["Temp-Identity-Type"] = authType;
        response.HttpContext.Items["Temp-Identity-Name"] = jwtUser;

        var request = new MUpdatePassword() { UserName = injectedUser! };

        var t = uc.UpdatePassword("temp", request);

        Assert.IsType<OkObjectResult>(t);
    }
}
