using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;

namespace Its.Onix.Api.Test.Services;

public class AuditLogServiceTest
{
    //===== GetAuditLogById() ====
    [Theory]
    [InlineData("org1")]
    public void GetAuditLogByIdTest(string orgId)
    {
        var auditLogId = Guid.NewGuid().ToString();
        var log = new MAuditLog() { CustomStatus = "test" };

        var auditLogRepo = new Mock<IAuditLogRepository>();
        auditLogRepo.Setup(s => s.GetAuditLogById(auditLogId)).Returns(log);

        var auditLogSvc = new AuditLogService(auditLogRepo.Object);
        var result = auditLogSvc.GetAuditLogById(orgId, auditLogId);

        Assert.NotNull(result);
        Assert.Equal("test", result.CustomStatus);
    }
    //=====

    //===== GetAuditLogCount() ====
    [Theory]
    [InlineData("org1")]
    public void GetAuditLogCountTest(string orgId)
    {
        var param = new VMAuditLog() { };

        var repo = new Mock<IAuditLogRepository>();
        repo.Setup(s => s.GetAuditLogCount(param)).Returns(5);

        var AuditLogSvc = new AuditLogService(repo.Object);
        var result = AuditLogSvc.GetAuditLogCount(orgId, param);

        Assert.Equal(5, result);
    }
    //=====

    //===== GetAuditLogs() ====
    [Theory]
    [InlineData("org1")]
    public void GetAuditLogsTest(string orgId)
    {
        var param = new VMAuditLog() { };

        var repo = new Mock<IAuditLogRepository>();
        repo.Setup(s => s.GetAuditLogs(param)).Returns([]);

        var AuditLogSvc = new AuditLogService(repo.Object);
        var result = AuditLogSvc.GetAuditLogs(orgId, param);

        Assert.Empty(result.ToArray());
    }
    //=====
}
