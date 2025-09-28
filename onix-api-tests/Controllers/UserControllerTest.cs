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
