using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Test.Services;

public class ScanItemActionServiceTest
{
    //===== GetScanItemActionById() ====
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    public void GetScanItemActionByIdTest(string orgId, string actionId)
    {
        var action = new MScanItemAction() { ThemeVerify = "xxxxx" };

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.GetScanItemActionById(actionId)).Returns(action);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.GetScanItemActionById(orgId, actionId);

        Assert.NotNull(result);
        Assert.Equal(action.ThemeVerify, action.ThemeVerify);
    }
    //=====

    [Theory]
    [InlineData("org1")]
    public void GetScanItemActionTest(string orgId)
    {
        var action = new MScanItemAction() { ThemeVerify = "xxxxx" };

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.GetScanItemAction()).Returns(action);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.GetScanItemAction(orgId);

        Assert.NotNull(result);
        Assert.Equal(action.ThemeVerify, action.ThemeVerify);
    }
    //=====

    //===== GetScanItemActionCount() ====
    [Theory]
    [InlineData("org1")]
    public void GetScanItemActionCountTest(string orgId)
    {
        var param = new VMScanItemAction() { FullTextSearch = "" };
        var action = new MScanItemAction() { ThemeVerify = "xxxxx" };

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.GetScanItemActionCount(param)).Returns(1);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.GetScanItemActionCount(orgId, param);

        Assert.Equal(1, result);
    }
    //=====

    //===== DeleteScanItemActionById() ====
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    public void DeleteScanItemActionByIdOkTest(string orgId, string actionId)
    {
        var action = new MScanItemAction() { ThemeVerify = "xxxxx" };

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.DeleteScanItemActionById(actionId)).Returns(action);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.DeleteScanItemActionById(orgId, actionId);

        Assert.Equal("OK", result!.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-XXXXX-eeeeeeeeeeee")]
    [InlineData("org1", "")]
    [InlineData("org1", "xxxx")]
    public void DeleteScanItemActionByIdInvalidIdTest(string orgId, string actionId)
    {
        var template = new MScanItemAction() { ThemeVerify = "xxxxx" };

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.DeleteScanItemActionById(actionId)).Returns(template);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.DeleteScanItemActionById(orgId, actionId);

        Assert.Equal("UUID_INVALID", result!.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-aaaa-eeeeeeeeeeee")]
    [InlineData("org1", "aaaaaaaa-dddd-cccc-aaaa-eeeeeeeeeeee")]
    public void DeleteScanItemActionByIdNotFoundTest(string orgId, string actionId)
    {
        MScanItemAction? template = null;

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.DeleteScanItemActionById(actionId)).Returns(template);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.DeleteScanItemActionById(orgId, actionId);

        Assert.Equal("NOTFOUND", result!.Status);
    }
    //=====

    //===== UpdateScanItemActionById() ====
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    public void UpdateScanItemActionByIdIdOkTest(string orgId, string actionId)
    {
        var template = new MScanItemAction() { ThemeVerify = "xxxx", EncryptionIV = "1234567890ABCDEF", EncryptionKey = "1234567890ABCDEF" };

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.UpdateScanItemActionById(actionId, template)).Returns(template);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.UpdateScanItemActionById(orgId, actionId, template);

        Assert.Equal("OK", result!.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-XXXXX-eeeeeeeeeeee")]
    [InlineData("org1", "")]
    [InlineData("org1", "xxxx")]
    public void UpdateScanItemActionByIdInvalidIdTest(string orgId, string actionId)
    {
        var template = new MScanItemAction() { ThemeVerify = "xxxxx", EncryptionIV = "1234567890ABCDEF", EncryptionKey = "1234567890ABCDEF" };

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.UpdateScanItemActionById(actionId, template)).Returns(template);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.UpdateScanItemActionById(orgId, actionId, template);

        Assert.Equal("UUID_INVALID", result!.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-aaaa-eeeeeeeeeeee")]
    [InlineData("org1", "aaaaaaaa-dddd-cccc-aaaa-eeeeeeeeeeee")]
    public void UpdateScanItemActionByIdNotFoundTest(string orgId, string actionId)
    {
        MScanItemAction? template = null;
        MScanItemAction inputAction = new() { ThemeVerify = "xxx", EncryptionIV = "1234567890ABCDEF", EncryptionKey = "1234567890ABCDEF" };

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.UpdateScanItemActionById(actionId, inputAction)).Returns(template);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.UpdateScanItemActionById(orgId, actionId, inputAction);

        Assert.Equal("NOTFOUND", result!.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-aaaa-eeeeeeeeeeee")]
    [InlineData("org1", "aaaaaaaa-dddd-cccc-aaaa-eeeeeeeeeeee")]
    public void UpdateScanItemActionByIdKeyErrorTest(string orgId, string actionId)
    {
        MScanItemAction? template = null;
        MScanItemAction inputAction = new() { ThemeVerify = "xxx", EncryptionIV = "1234567890ABCDEF", EncryptionKey = "1234567890" };

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.UpdateScanItemActionById(actionId, inputAction)).Returns(template);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.UpdateScanItemActionById(orgId, actionId, inputAction);

        Assert.Equal("ERROR_KEY_TOO_SHORT", result!.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-aaaa-eeeeeeeeeeee")]
    [InlineData("org1", "aaaaaaaa-dddd-cccc-aaaa-eeeeeeeeeeee")]
    public void UpdateScanItemActionByIdIvErrorTest(string orgId, string actionId)
    {
        MScanItemAction? template = null;
        MScanItemAction inputAction = new() { ThemeVerify = "xxx", EncryptionIV = "1234567890", EncryptionKey = "1234567890ABCDEF" };

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.UpdateScanItemActionById(actionId, inputAction)).Returns(template);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.UpdateScanItemActionById(orgId, actionId, inputAction);

        Assert.Equal("ERROR_KEY_TOO_SHORT", result!.Status);
    }
    //=====

    //===== AddScanItemAction()
    [Theory]
    [InlineData("org1", "test")]
    [InlineData("org1", "a")]
    [InlineData("org1", "xxx")]
    public void AddScanItemActionMultipleTest(string orgId, string theme)
    {
        var template = new MScanItemAction() { ThemeVerify = theme, EncryptionIV = "1234567890ABCDEF", EncryptionKey = "1234567890ABCDEF" };

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.GetScanItemActionCount(It.IsAny<VMScanItemAction>())).Returns(1);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.AddScanItemAction(orgId, template);

        Assert.Equal("NOT_ALLOW_MORE_THAN_ONE", result!.Status);
    }

    [Theory]
    [InlineData("org1", "test")]
    [InlineData("org1", "a")]
    [InlineData("org1", "xxx")]
    public void AddScanItemActionOkTest(string orgId, string theme)
    {
        var template = new MScanItemAction() { ThemeVerify = theme, EncryptionIV = "1234567890ABCDEF", EncryptionKey = "1234567890ABCDEF" };

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.GetScanItemActionCount(It.IsAny<VMScanItemAction>())).Returns(0);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.AddScanItemAction(orgId, template);

        Assert.Equal("OK", result!.Status);
    }

    [Theory]
    [InlineData("org1", "test")]
    [InlineData("org1", "a")]
    [InlineData("org1", "xxx")]
    public void AddScanItemActionKeyErrorTest(string orgId, string theme)
    {
        var template = new MScanItemAction() { ThemeVerify = theme, EncryptionIV = "1234567890ABCDEF", EncryptionKey = "1234567890" };

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.GetScanItemActionCount(It.IsAny<VMScanItemAction>())).Returns(0);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.AddScanItemAction(orgId, template);

        Assert.Equal("ERROR_KEY_TOO_SHORT", result!.Status);
    }

    [Theory]
    [InlineData("org1", "test")]
    [InlineData("org1", "a")]
    [InlineData("org1", "xxx")]
    public void AddScanItemActionIvErrorTest(string orgId, string theme)
    {
        var template = new MScanItemAction() { ThemeVerify = theme, EncryptionIV = "1234567890", EncryptionKey = "1234567890ABCDEF" };

        var redisHelper = new Mock<IRedisHelper>();

        var sciActRepo = new Mock<IScanItemActionRepository>();
        sciActRepo.Setup(s => s.GetScanItemActionCount(It.IsAny<VMScanItemAction>())).Returns(0);

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.AddScanItemAction(orgId, template);

        Assert.Equal("ERROR_KEY_TOO_SHORT", result!.Status);
    }
    //=====

    //===== GetScanItemActionDefault()
    [Theory]
    [InlineData("org1")]
    [InlineData("org2")]
    [InlineData("org3")]
    public void GetScanItemActionDefaultOkTest(string orgId)
    {
        var redisHelper = new Mock<IRedisHelper>();
        var sciActRepo = new Mock<IScanItemActionRepository>();

        var sciActSvc = new ScanItemActionService(sciActRepo.Object, redisHelper.Object);
        var result = sciActSvc.GetScanItemActionDefault(orgId);

        Assert.NotNull(result.EncryptionIV);
        Assert.NotNull(result.EncryptionKey);

        Assert.Equal(16, result.EncryptionIV.Length);
        Assert.Equal(16, result.EncryptionKey.Length);
        Assert.Equal("default", result.ThemeVerify);
        Assert.Equal("TRUE", result.RegisteredAwareFlag);

        //เพื่อป้องกัน string interpolate
        var keyword = $"https://verify-dev.please-scan.com/verify";
        Assert.Contains(keyword, result.RedirectUrl);
    }
    //=====
}
