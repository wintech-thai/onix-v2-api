using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Its.Onix.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Its.Onix.Api.ModelsViews;
using System.Text.Json;

namespace Its.Onix.Api.Test.Controllers;

public class UserControllerTest
{
/*  
    //TODO : Move this to OnlyUserControllerTest
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

    //TODO : Move this to OnlyUserControllerTest
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
*/
    [Fact]
    public void AddUserSuccessTest()
    {
        var service = new Mock<IUserService>();
        service.Setup(s => s.AddUser(It.IsAny<string>(), It.IsAny<MUser>())).Returns(new MVUser()
        {
            Status = "OK",
        });

        var uc = new UserController(service.Object);

        var request = new MUser();
        var t = uc.AddUser("temp", request);

        var result = Assert.IsType<OkObjectResult>(t);
        var user = Assert.IsType<MVUser>(result.Value);
        Assert.Equal("OK", user.Status);
    }

    [Theory]
    [InlineData("ERROR")]
    [InlineData("NOTSUCCCESS")]
    [InlineData("FAIL")]
    public void AddUserErrorTest(string status)
    {
        var service = new Mock<IUserService>();
        service.Setup(s => s.AddUser(It.IsAny<string>(), It.IsAny<MUser>())).Returns(new MVUser()
        {
            Status = status,
            Description = status,
        });

        var uc = new UserController(service.Object);

        var request = new MUser();
        var t = uc.AddUser("temp", request);

        var result = Assert.IsType<BadRequestObjectResult>(t);
        var errMsg = Assert.IsType<string>(result.Value);
        Assert.NotNull(errMsg);
        Assert.NotEmpty(errMsg);
        Assert.Equal(status, errMsg);
    }
}
