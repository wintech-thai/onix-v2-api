using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;
using Microsoft.AspNetCore.Mvc;

namespace Its.Onix.Api.Test.Controllers;

public class JobControllerTest
{
    //=== CreateJobScanItemGenerator() ===
    [Theory]
    [InlineData("org1")]
    public void CreateJobScanItemGeneratorNoScanItemTemplateTest(string orgId)
    {
        var sciService = new Mock<IScanItemTemplateService>();
        sciService.Setup(s => s.GetScanItemTemplate(orgId)).Returns((MScanItemTemplate)null!);

        var service = new Mock<IJobService>();
        var jc = new JobController(service.Object, sciService.Object);
        jc.ControllerContext.HttpContext = new DefaultHttpContext();

        var request = new MJob();
        var result = jc.CreateJobScanItemGenerator(orgId, request);

        var custStatus = jc.Response.Headers["CUST_STATUS"];

        Assert.NotNull(result);
        Assert.Equal("NO_SCAN_ITEM_TEMPLATE_FOUND", result.Status);
        Assert.Equal("NO_SCAN_ITEM_TEMPLATE", custStatus);
    }

    [Theory]
    [InlineData("org1")]
    public void CreateJobScanItemGeneratorOkTest(string orgId)
    {
        var sciTemplate = new MScanItemTemplate() { };
        var job = new MJob()
        {
            Parameters = [new NameValue() { Name = "EMAIL_NOTI_ADDRESS", Value = "test@gmail.com" }]
        };

        var mvJob = new MVJob() { Status = "OK" };

        var sciService = new Mock<IScanItemTemplateService>();
        sciService.Setup(s => s.GetScanItemTemplate(orgId)).Returns(sciTemplate);

        var service = new Mock<IJobService>();
        service.Setup(s => s.AddJob(orgId, job)).Returns(mvJob);

        var jc = new JobController(service.Object, sciService.Object);
        jc.ControllerContext.HttpContext = new DefaultHttpContext();

        var result = jc.CreateJobScanItemGenerator(orgId, job);

        var custStatus = jc.Response.Headers["CUST_STATUS"];

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //======

    //=== CreateJobCacheLoaderTrigger() ===
    [Theory]
    [InlineData("org1")]
    public void CreateJobCacheLoaderTriggerOkTest(string orgId)
    {
        var job = new MJob()
        {
            Parameters = [new NameValue() { Name = "DATA_SECTION", Value = "ALL" }]
        };

        var mvJob = new MVJob() { Status = "OK" };

        var sciService = new Mock<IScanItemTemplateService>();

        var service = new Mock<IJobService>();
        service.Setup(s => s.AddJob(orgId, job)).Returns(mvJob);

        var jc = new JobController(service.Object, sciService.Object);
        var result = jc.CreateJobCacheLoaderTrigger(orgId, job);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //===

    //=== CreateJobOtpEmailSend() ===
    [Theory]
    [InlineData("org1")]
    public void CreateJobOtpEmailSendOkTest(string orgId)
    {
        var job = new MJob();
        var mvJob = new MVJob() { Status = "OK" };

        var sciService = new Mock<IScanItemTemplateService>();

        var service = new Mock<IJobService>();
        service.Setup(s => s.AddJob(orgId, job)).Returns(mvJob);

        var jc = new JobController(service.Object, sciService.Object);
        var result = jc.CreateJobOtpEmailSend(orgId, job);

        //ทดสอบว่าเป็น NULL เพราะว่า เราไม่ต้องการเปิด API เส้นนี้ให้ใช้งานได้ 
        //API เส้นนี้ทำไว้ทดสอบเป็นครั้งคราวเท่านั้น
        Assert.Null(result);
    }
    //===

    //=== GetJobCount() ===
    [Theory]
    [InlineData("org1")]
    public void GetJobCountOkTest(string orgId)
    {
        var job = new VMJob();
        var mvJob = new MVJob() { Status = "OK" };

        var sciService = new Mock<IScanItemTemplateService>();

        var service = new Mock<IJobService>();
        service.Setup(s => s.GetJobCount(orgId, job)).Returns(5);

        var jc = new JobController(service.Object, sciService.Object);
        var result = jc.GetJobCount(orgId, job);

        var t = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<int>(t.Value);
        Assert.Equal(5, t.Value);
    }
    //===

    //=== GetJobById() ===
    [Theory]
    [InlineData("org1", "id1")]
    public void GetJobByIdOkTest(string orgId, string jobId)
    {
        var job = new MJob() { Name = jobId };

        var sciService = new Mock<IScanItemTemplateService>();

        var service = new Mock<IJobService>();
        service.Setup(s => s.GetJobById(orgId, jobId)).Returns(job);

        var jc = new JobController(service.Object, sciService.Object);
        var result = jc.GetJobById(orgId, jobId);

        Assert.IsType<MJob>(result);
        Assert.Equal(jobId, result.Name);
    }
    //===

    //=== GetJobs() ===
    [Theory]
    [InlineData("org1")]
    public void GetJobsOkTest(string orgId)
    {
        var job = new VMJob() { FullTextSearch = "", Limit = 0 };

        var sciService = new Mock<IScanItemTemplateService>();

        var service = new Mock<IJobService>();
        service.Setup(s => s.GetJobs(orgId, job)).Returns([]);

        var jc = new JobController(service.Object, sciService.Object);
        var result = jc.GetJobs(orgId, job);

        Assert.NotNull(result);
        var t = Assert.IsType<OkObjectResult>(result);
    }
    //===

    //=== GetJobDefault.ScanItemGenerator() ===
    [Theory]
    [InlineData("org1")]
    public void GetJobDefaultScanItemGeneratorTest(string orgId)
    {
        var job = new MJob() { Name = "HELLO" };
        var sciService = new Mock<IScanItemTemplateService>();

        var service = new Mock<IJobService>();
        service.Setup(s => s.GetJobTemplate(orgId, "ScanItemGenerator", It.IsAny<string>())).Returns(job);

        var jc = new JobController(service.Object, sciService.Object);
        jc.ControllerContext.HttpContext = new DefaultHttpContext();
        jc.Response.HttpContext.Items["Temp-Identity-Name"] = "user1";

        var result = jc.GetJobDefault(orgId);

        Assert.NotNull(result);
        Assert.Equal("HELLO", result.Name);
    }
    //===
}
