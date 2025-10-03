using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Test.Services;

public class ScanItemTemplateServiceTest
{
    //===== GetScanItemTemplateById() ====
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    public void GetScanItemTemplateByIdTest(string orgId, string templateId)
    {
        var template = new MScanItemTemplate() { NotificationEmail = "xxxxx" };

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciTplRepo = new Mock<IScanItemTemplateRepository>();
        sciTplRepo.Setup(s => s.GetScanItemTemplateById(templateId)).Returns(template);

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.GetScanItemTemplateById(orgId, templateId);

        Assert.NotNull(result);
        Assert.Equal(template.NotificationEmail, result.NotificationEmail);
    }
    //=====

    //===== GetScanItemTemplateById() ====
    [Theory]
    [InlineData("org1")]
    public void GetScanItemTemplateTest(string orgId)
    {
        var template = new MScanItemTemplate() { NotificationEmail = "xxxxx" };

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciTplRepo = new Mock<IScanItemTemplateRepository>();
        sciTplRepo.Setup(s => s.GetScanItemTemplate()).Returns(template);

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.GetScanItemTemplate(orgId);

        Assert.NotNull(result);
        Assert.Equal(template.NotificationEmail, result.NotificationEmail);
    }
    //=====

    //===== GetScanItemTemplateCount() ====
    [Theory]
    [InlineData("org1")]
    public void GetScanItemTemplateCountTest(string orgId)
    {
        var param = new VMScanItemTemplate() { FullTextSearch = "" };
        var template = new MScanItemTemplate() { NotificationEmail = "xxxxx" };

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciTplRepo = new Mock<IScanItemTemplateRepository>();
        sciTplRepo.Setup(s => s.GetScanItemTemplateCount(param)).Returns(1);

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.GetScanItemTemplateCount(orgId, param);

        Assert.Equal(1, result);
    }
    //=====

    //===== DeleteScanItemTemplateById() ====
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    public void DeleteScanItemTemplateByIdOkTest(string orgId, string templateId)
    {
        var template = new MScanItemTemplate() { NotificationEmail = "xxxxx" };

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciTplRepo = new Mock<IScanItemTemplateRepository>();
        sciTplRepo.Setup(s => s.DeleteScanItemTemplateById(templateId)).Returns(template);

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.DeleteScanItemTemplateById(orgId, templateId);

        Assert.Equal("OK", result!.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-XXXXX-eeeeeeeeeeee")]
    [InlineData("org1", "")]
    [InlineData("org1", "xxxx")]
    public void DeleteScanItemTemplateByIdInvalidIdTest(string orgId, string templateId)
    {
        var template = new MScanItemTemplate() { NotificationEmail = "xxxxx" };

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciTplRepo = new Mock<IScanItemTemplateRepository>();
        sciTplRepo.Setup(s => s.DeleteScanItemTemplateById(templateId)).Returns(template);

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.DeleteScanItemTemplateById(orgId, templateId);

        Assert.Equal("UUID_INVALID", result!.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-aaaa-eeeeeeeeeeee")]
    [InlineData("org1", "aaaaaaaa-dddd-cccc-aaaa-eeeeeeeeeeee")]
    public void DeleteScanItemTemplateByIdNotFoundTest(string orgId, string templateId)
    {
        MScanItemTemplate? template = null;

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciTplRepo = new Mock<IScanItemTemplateRepository>();
        sciTplRepo.Setup(s => s.DeleteScanItemTemplateById(templateId)).Returns(template);

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.DeleteScanItemTemplateById(orgId, templateId);

        Assert.Equal("NOTFOUND", result!.Status);
    }
    //=====


    //===== UpdateScanItemTemplateById() ====
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    public void UpdateScanItemTemplateByIdIdOkTest(string orgId, string templateId)
    {
        var template = new MScanItemTemplate() { NotificationEmail = "xxx@gmail.com" };

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciTplRepo = new Mock<IScanItemTemplateRepository>();
        sciTplRepo.Setup(s => s.UpdateScanItemTemplateById(templateId, template)).Returns(template);

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.UpdateScanItemTemplateById(orgId, templateId, template);

        Assert.Equal("OK", result!.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-XXXXX-eeeeeeeeeeee")]
    [InlineData("org1", "")]
    [InlineData("org1", "xxxx")]
    public void UpdateScanItemTemplateByIdInvalidIdTest(string orgId, string templateId)
    {
        var template = new MScanItemTemplate() { NotificationEmail = "xxxxx" };

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciTplRepo = new Mock<IScanItemTemplateRepository>();
        sciTplRepo.Setup(s => s.UpdateScanItemTemplateById(templateId, template)).Returns(template);

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.UpdateScanItemTemplateById(orgId, templateId, template);

        Assert.Equal("UUID_INVALID", result!.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-dddd-cccc-aaaa-eeeeeeeeeeee", "xxxx.com")]
    [InlineData("org1", "aaaaaaaa-dddd-cccc-aaaa-eeeeeeeeeeee", "a@s")]
    [InlineData("org1", "aaaaaaaa-dddd-cccc-aaaa-eeeeeeeeeeee", "xxx@com")]
    [InlineData("org1", "aaaaaaaa-dddd-cccc-aaaa-eeeeeeeeeeee", null)]
    public void UpdateScanItemTemplateByIdEmailErrorTest(string orgId, string templateId, string email)
    {
        var template = new MScanItemTemplate() { NotificationEmail = email };

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciTplRepo = new Mock<IScanItemTemplateRepository>();
        sciTplRepo.Setup(s => s.UpdateScanItemTemplateById(templateId, template)).Returns(template);

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.UpdateScanItemTemplateById(orgId, templateId, template);

        Assert.Equal("ERROR_VALIDATION_EMAIL", result!.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-aaaa-eeeeeeeeeeee")]
    [InlineData("org1", "aaaaaaaa-dddd-cccc-aaaa-eeeeeeeeeeee")]
    public void UpdateScanItemTemplateByIdNotFoundTest(string orgId, string templateId)
    {
        MScanItemTemplate? template = null;
        MScanItemTemplate inputTemplate = new() { NotificationEmail = "xxx@gmail.com" };

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciTplRepo = new Mock<IScanItemTemplateRepository>();
        sciTplRepo.Setup(s => s.UpdateScanItemTemplateById(templateId, inputTemplate)).Returns(template);

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.UpdateScanItemTemplateById(orgId, templateId, inputTemplate);

        Assert.Equal("NOTFOUND", result!.Status);
    }
    //=====

    //===== AddScanItemTemplate()
    [Theory]
    [InlineData("org1", "xxxx.com")]
    [InlineData("org1", "a@s")]
    [InlineData("org1", "xxx@com")]
    [InlineData("org1", null)]
    public void AddScanItemTemplateEmailErrorTest(string orgId, string email)
    {
        var template = new MScanItemTemplate() { NotificationEmail = email };

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciTplRepo = new Mock<IScanItemTemplateRepository>();
        sciTplRepo.Setup(s => s.AddScanItemTemplate(template)).Returns(template);

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.AddScanItemTemplate(orgId, template);

        Assert.Equal("ERROR_VALIDATION_EMAIL", result!.Status);
    }

    [Theory]
    [InlineData("org1", "test@xxxx.com")]
    [InlineData("org1", "a@s.com")]
    [InlineData("org1", "xxx@abc.com")]
    public void AddScanItemTemplateMultipleTest(string orgId, string email)
    {
        var template = new MScanItemTemplate() { NotificationEmail = email };

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciTplRepo = new Mock<IScanItemTemplateRepository>();
        sciTplRepo.Setup(s => s.GetScanItemTemplateCount(It.IsAny<VMScanItemTemplate>())).Returns(1);

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.AddScanItemTemplate(orgId, template);

        Assert.Equal("NOT_ALLOW_MORE_THAN_ONE", result!.Status);
    }

    [Theory]
    [InlineData("org1", "test@xxxx.com")]
    [InlineData("org1", "a@s.com")]
    [InlineData("org1", "xxx@abc.com")]
    public void AddScanItemTemplateOkTest(string orgId, string email)
    {
        var template = new MScanItemTemplate() { NotificationEmail = email };

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciTplRepo = new Mock<IScanItemTemplateRepository>();
        sciTplRepo.Setup(s => s.GetScanItemTemplateCount(It.IsAny<VMScanItemTemplate>())).Returns(0);

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.AddScanItemTemplate(orgId, template);

        Assert.Equal("OK", result!.Status);
    }
    //=====

    //===== AddScanItemTemplate()
    [Theory]
    [InlineData("org1", "xxxx.com")]
    [InlineData("org1", "a@s")]
    [InlineData("org1", "xxx@com")]
    public void GetScanItemTemplateDefaultNoUserTest(string orgId, string userName)
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(s => s.GetUserByName(userName)).Returns((MUser?)null!);

        var redisHelper = new Mock<IRedisHelper>();
        var sciTplRepo = new Mock<IScanItemTemplateRepository>();

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.GetScanItemTemplateDefault(orgId, userName);

        Assert.NotNull(result);
        Assert.Equal("your-email@email-xxx.com", result.NotificationEmail);
    }

    [Theory]
    [InlineData("org1", "xxxx.com", "xxx@yyyy.com")]
    [InlineData("org1", "a@s", "1234@yyyy.com")]
    [InlineData("org1", "xxx@com", "xxx@ssdasd.com")]
    public void GetScanItemTemplateDefaultOkTest(string orgId, string userName, string email)
    {
        var user = new MUser() { UserEmail = email };
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(s => s.GetUserByName(userName)).Returns(user);

        var redisHelper = new Mock<IRedisHelper>();
        var sciTplRepo = new Mock<IScanItemTemplateRepository>();

        var sciTplSvc = new ScanItemTemplateService(sciTplRepo.Object, redisHelper.Object, userRepo.Object);
        var result = sciTplSvc.GetScanItemTemplateDefault(orgId, userName);

        Assert.NotNull(result);
        Assert.Equal(email, result.NotificationEmail);

        Assert.Equal(2, result.SerialPrefixDigit);
        Assert.Equal(100, result.GeneratorCount);
        Assert.Equal(7, result.SerialDigit);
        Assert.Equal(7, result.PinDigit);

        //เพื่อป้องกัน string interpolate
        var serial = "{VAR_SERIAL}";
        var pin = "{VAR_PIN}";
        var keyword = $"org/{orgId}/Verify/{serial}/{pin}";
        Assert.Contains(keyword, result.UrlTemplate);
    }
    //=====
}
