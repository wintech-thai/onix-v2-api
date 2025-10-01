using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Test.Services;

public class JobServiceTest
{
    //===== GetJobById() ====
    [Theory]
    [InlineData("org1")]
    public void GetJobByIdTest(string orgId)
    {
        var jobId = Guid.NewGuid().ToString();
        var job = new MJob() { Name = "jobnamehere" };

        var repo = new Mock<IJobRepository>();
        repo.Setup(s => s.GetJobById(jobId)).Returns(job);

        var redisHelper = new Mock<IRedisHelper>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object);
        var result = jobSvc.GetJobById(orgId, jobId);

        Assert.NotNull(result);
        Assert.Equal("jobnamehere", result.Name);
    }
    //=====

    //===== GetJobCount() ====
    [Theory]
    [InlineData("org1")]
    public void GetJobCountTest(string orgId)
    {
        var param = new VMJob() { };

        var repo = new Mock<IJobRepository>();
        repo.Setup(s => s.GetJobCount(param)).Returns(5);

        var redisHelper = new Mock<IRedisHelper>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object);
        var result = jobSvc.GetJobCount(orgId, param);

        Assert.Equal(5, result);
    }
    //=====

    //===== GetJobs() ====
    [Theory]
    [InlineData("org1")]
    public void GetJobsTest(string orgId)
    {
        var param = new VMJob() { };

        var repo = new Mock<IJobRepository>();
        repo.Setup(s => s.GetJobs(param)).Returns([]);

        var redisHelper = new Mock<IRedisHelper>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object);
        var result = jobSvc.GetJobs(orgId, param);

        Assert.Empty(result.ToArray());
    }
    //=====

    //===== AddJob() ====
    [Theory]
    [InlineData("org1", "JOB1")]
    public void AddJobTest(string orgId, string jobName)
    {
        var job = new MJob() { Name = jobName, Description = jobName };

        var repo = new Mock<IJobRepository>();
        repo.Setup(s => s.AddJob(job)).Returns(job);

        var redisHelper = new Mock<IRedisHelper>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object);
        var result = jobSvc.AddJob(orgId, job);

        Assert.NotNull(result);
        Assert.NotNull(result.Job);
        Assert.Equal(jobName, result.Job.Name);
    }
    //=====
}
