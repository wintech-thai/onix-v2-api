using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Its.Onix.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.ModelsViews;

namespace Its.Onix.Api.Test.Controllers;

public class UserControllerTest
{
    [Theory]
    [InlineData("org1")]
    [InlineData("org2")]
    public void UpdatePasswordNoIdentityTypeTest(string orgId)
    {
        IUserService service = Mock.Of<IUserService>();
        var uc = new UserController(service);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();
        //var response = uc.ControllerContext.HttpContext.Response;
        //response.HttpContext.Items["Temp-Identity-Type"]

        var request = new MUpdatePassword();

        var t = uc.UpdatePassword(orgId, request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(t);
        Assert.Contains("Unable to identify identity type", badRequestResult.Value!.ToString());
    }

    [Theory]
    [InlineData("API-KEY")]
    [InlineData("XXX")]
    public void UpdatePasswordNotJwtTest(string authType)
    {
        IUserService service = Mock.Of<IUserService>();
        var uc = new UserController(service);

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
        IUserService service = Mock.Of<IUserService>();
        var uc = new UserController(service);

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
        IUserService service = Mock.Of<IUserService>();
        var uc = new UserController(service);

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
        var service = new Mock<IUserService>();
        service.Setup(s => s.UpdatePassword(It.IsAny<string>(), It.IsAny<MUpdatePassword>())).Returns(new MVUpdatePassword()
        {
            Status = "SUCCESS",
        });

        var uc = new UserController(service.Object);

        uc.ControllerContext.HttpContext = new DefaultHttpContext();

        var response = uc.ControllerContext.HttpContext.Response;
        response.HttpContext.Items["Temp-Identity-Type"] = authType;
        response.HttpContext.Items["Temp-Identity-Name"] = jwtUser;

        var request = new MUpdatePassword() { UserName = injectedUser! };

        var t = uc.UpdatePassword("temp", request);

        Assert.IsType<OkObjectResult>(t);
    }
}
