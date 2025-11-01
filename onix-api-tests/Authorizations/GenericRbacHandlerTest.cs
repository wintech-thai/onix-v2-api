using Moq;
using Its.Onix.Api.Services;
using Microsoft.AspNetCore.Http;
using Its.Onix.Api.Models;
using Its.Onix.Api.Authorizations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Its.Onix.Api.Test.Authentications;

public class GenericRbacHandlerTest
{
    [Theory]
    [InlineData("org1", "/api/User/org/org1/action/AddUser", "OWNER")]
    [InlineData("org2", "/api/OnlyUser/org/org2/action/GetUserAllowedOrg", "TEMP")] // API นี้ไม่ต้องการสิทธิ์
    [InlineData("org2", "/api/OnlyUser/org/org2/action/UpdatePassword", "TEMP")] // API นี้ไม่ต้องการสิทธิ์
    [InlineData("global", "/api/OnlyAdmin/org/global/action/UpdatePassword", "OWNER")] // Admin only API
    public async Task HandleRequirementTest(string orgId, string uri, string roleMatch)
    {
        var random = new Random();
        var code = random.Next(0, 1000000).ToString("D6");

        var roleSvc = new Mock<IRoleService>();
        roleSvc.Setup(s => s.GetRolesList("", "OWNER")).Returns(
        [
            new MRole
            {
                RoleName = "OWNER",
                RoleDefinition = ".+:.+" // ตรงกับ group:api
            }
        ]);

        var handler = new GenericRbacHandler(roleSvc.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Role, "OWNER"),
            new(ClaimTypes.Uri, uri),
            new(ClaimTypes.AuthenticationMethod, code), // ใช้ทดสอบว่า claim นี้ถูกเพิ่มเข้ามา
            new(ClaimTypes.GroupSid, orgId),
            new(ClaimTypes.Name, "John Doe")
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        var requirement = new GenericRbacRequirement();
        var context = new AuthorizationHandlerContext(
            [requirement],
            user,
            new DefaultHttpContext() // resource
        );

        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);

        var parts = uri.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var apiName = parts[5]; // ดึงชื่อ API จาก URI
        var controllerName = parts[1]; // ดึงชื่อ Controller จาก URI

        var httpContext = (DefaultHttpContext)context.Resource!;
        Assert.Equal(code, httpContext.Items["Temp-Identity-Type"]);
        Assert.Equal(roleMatch, httpContext.Items["Temp-Authorized-Role"]);
        Assert.Equal($"{controllerName}:{apiName}", httpContext.Items["Temp-API-Called"]);
    }

    [Theory]
    [InlineData("org1", "/api/User/org/org1/action/AddUser")]
    [InlineData("org1", "/api/OnlyAdmin/org/org1/action/OnlyAdminApiHere")]
    public async Task HandleRequirementRoleNotMatchTest(string orgId, string uri)
    {
        var userRole = "DUMMY";
        var random = new Random();
        var code = random.Next(0, 1000000).ToString("D6");

        var roleSvc = new Mock<IRoleService>();
        roleSvc.Setup(s => s.GetRolesList("", "TESTING")).Returns(
        [
            new MRole
            {
                RoleName = userRole,
                RoleDefinition = "Dymmy:Test.+" // ตรงกับ group:api
            }
        ]);

        var handler = new GenericRbacHandler(roleSvc.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Role, userRole),
            new(ClaimTypes.Uri, uri),
            new(ClaimTypes.AuthenticationMethod, code), // ใช้ทดสอบว่า claim นี้ถูกเพิ่มเข้ามา
            new(ClaimTypes.GroupSid, orgId),
            new(ClaimTypes.Name, "John Doe")
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        var requirement = new GenericRbacRequirement();
        var context = new AuthorizationHandlerContext(
            [requirement],
            user,
            new DefaultHttpContext() // resource
        );

        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);

        var httpContext = (DefaultHttpContext)context.Resource!;
/*
        //TODO : Test this
        Assert.NotNull(httpContext.Items["Temp-Identity-Type"]);
        Assert.NotNull(httpContext.Items["Temp-Authorized-Role"]);
        Assert.NotNull(httpContext.Items["Temp-API-Called"]);
*/
    }

    [Theory]
    [InlineData("org1", "/api/User/org/org1/action/AddUser")]
    [InlineData("org1", "/api/OnlyAdmin/org/org1/action/OnlyAdminApiHere")] 
    public async Task HandleRequirementRolesIsEmptyTest(string orgId, string uri)
    {
        var userRole = "DUMMY";
        var random = new Random();
        var code = random.Next(0, 1000000).ToString("D6");

        var roleSvc = new Mock<IRoleService>();
        roleSvc.Setup(s => s.GetRolesList(It.IsAny<string>(), It.IsAny<string>())).Returns([]);

        var handler = new GenericRbacHandler(roleSvc.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Role, userRole),
            new(ClaimTypes.Uri, uri),
            new(ClaimTypes.AuthenticationMethod, code), // ใช้ทดสอบว่า claim นี้ถูกเพิ่มเข้ามา
            new(ClaimTypes.GroupSid, orgId),
            new(ClaimTypes.Name, "John Doe")
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        var requirement = new GenericRbacRequirement();
        var context = new AuthorizationHandlerContext(
            [requirement],
            user,
            new DefaultHttpContext() // resource
        );

        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
/*
        //TODO : Test this
        var httpContext = (DefaultHttpContext)context.Resource!;
        Assert.NotNull(httpContext.Items["Temp-Identity-Type"]);
        Assert.NotNull(httpContext.Items["Temp-Authorized-Role"]);
        Assert.NotNull(httpContext.Items["Temp-API-Called"]);
*/
    }

    [Theory]
    [InlineData("org1", "/api/User/org/org1/action/AddUser")]
    [InlineData("org1", "/api/OnlyAdmin/org/org1/action/OnlyAdminApiHere")]
    public async Task HandleRequirementAllRoleNotMatchTest(string orgId, string uri)
    {
        var userRole = "DUMMY";
        var random = new Random();
        var code = random.Next(0, 1000000).ToString("D6");

        var roleSvc = new Mock<IRoleService>();
        roleSvc.Setup(s => s.GetRolesList(It.IsAny<string>(), It.IsAny<string>())).Returns(
        [
            new MRole
            {
                RoleName = userRole,
                RoleDefinition = "xxx,xxx"
            }
        ]);

        var handler = new GenericRbacHandler(roleSvc.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Role, userRole),
            new(ClaimTypes.Uri, uri),
            new(ClaimTypes.AuthenticationMethod, code), // ใช้ทดสอบว่า claim นี้ถูกเพิ่มเข้ามา
            new(ClaimTypes.GroupSid, orgId),
            new(ClaimTypes.Name, "John Doe")
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        var requirement = new GenericRbacRequirement();
        var context = new AuthorizationHandlerContext(
            [requirement],
            user,
            new DefaultHttpContext() // resource
        );

        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
/*
        //TODO : Test this
        var httpContext = (DefaultHttpContext)context.Resource!;
        Assert.NotNull(httpContext.Items["Temp-Identity-Type"]);
        Assert.NotNull(httpContext.Items["Temp-Authorized-Role"]);
        Assert.NotNull(httpContext.Items["Temp-API-Called"]);
*/
    }

    [Theory]
    [InlineData(null, "", "", "", "", "")]
    [InlineData("", null, "", "", "", "")]
    [InlineData("", "", null, "", "", "")]
    [InlineData("", "", "", null, "", "")]
    [InlineData("", "", "", "", null, "")]
    [InlineData("", "", "", "", "", null)]
    public async Task HandleRequirementClaimsIsNullTest(string name, string role, string path, string method, string group, string fullName)
    {
        var roleSvc = new Mock<IRoleService>();
        roleSvc.Setup(s => s.GetRolesList(It.IsAny<string>(), It.IsAny<string>())).Returns([]);

        var handler = new GenericRbacHandler(roleSvc.Object);

        string[] arrs = [
            name,
            role,
            path,
            method,
            group,
            fullName
        ];

        string[] types = [
            ClaimTypes.NameIdentifier,
            ClaimTypes.Role,
            ClaimTypes.Uri,
            ClaimTypes.AuthenticationMethod,
            ClaimTypes.GroupSid,
            ClaimTypes.Name
        ];

        var claims = new List<Claim>();

        int index = 0;
        foreach (var item in arrs)
        {
            if (item != null)
            {
                var claim = new Claim(types[index], item);
                claims.Add(claim);
            }

            index++;
        }

        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        var requirement = new GenericRbacRequirement();
        var context = new AuthorizationHandlerContext(
            [requirement],
            user,
            new DefaultHttpContext() // resource
        );

        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);

        var httpContext = (DefaultHttpContext)context.Resource!;
        Assert.Null(httpContext.Items["Temp-Identity-Type"]);
        Assert.Null(httpContext.Items["Temp-Authorized-Role"]);
        Assert.Null(httpContext.Items["Temp-API-Called"]);
    }
}
