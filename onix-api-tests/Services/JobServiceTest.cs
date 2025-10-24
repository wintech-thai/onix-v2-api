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
    [InlineData("org1", "")]
    [InlineData("org1", "[]")]
    public void GetJobByIdTest(string orgId, string jobConfig)
    {
        var jobId = Guid.NewGuid().ToString();
        var job = new MJob() { Name = "jobnamehere", Configuration = jobConfig };

        var repo = new Mock<IJobRepository>();
        repo.Setup(s => s.GetJobById(jobId)).Returns(job);

        var userRepo = new Mock<IUserRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object, userRepo.Object);
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
        var userRepo = new Mock<IUserRepository>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object, userRepo.Object);
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
        var userRepo = new Mock<IUserRepository>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object, userRepo.Object);
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
        var userRepo = new Mock<IUserRepository>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object, userRepo.Object);
        var result = jobSvc.AddJob(orgId, job);

        Assert.NotNull(result);
        Assert.NotNull(result.Job);
        Assert.Equal(jobName, result.Job.Name);
    }

    [Theory]
    [InlineData("org1", "JOB1", "email.com")]
    [InlineData("org1", "JOB1", "email")]
    [InlineData("org1", "JOB1", "x@com")]
    public void AddJobValidateEmailErrorTest(string orgId, string jobName, string email)
    {
        var job = new MJob()
        {
            Name = jobName,
            Description = jobName,
            Parameters = [new NameValue() { Name = "EMAIL_NOTI_ADDRESS", Value = email }]
        };

        var repo = new Mock<IJobRepository>();
        repo.Setup(s => s.AddJob(job)).Returns(job);

        var redisHelper = new Mock<IRedisHelper>();
        var userRepo = new Mock<IUserRepository>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object, userRepo.Object);
        var result = jobSvc.AddJob(orgId, job);

        Assert.NotNull(result);
        Assert.Null(result.Job);
        Assert.Equal("ERROR_VALIDATION_EMAIL", result.Status);
    }

    [Theory]
    [InlineData("org1", "JOB1")]
    public void AddJobParametersEmptyTest(string orgId, string jobName)
    {
        var job = new MJob()
        {
            Name = jobName,
            Description = jobName,
            Parameters = [new NameValue() { Name = "XXXXXX", Value = "xxxx" }]
        };

        var repo = new Mock<IJobRepository>();
        repo.Setup(s => s.AddJob(job)).Returns(job);

        var redisHelper = new Mock<IRedisHelper>();
        var userRepo = new Mock<IUserRepository>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object, userRepo.Object);
        var result = jobSvc.AddJob(orgId, job);

        Assert.NotNull(result);
        Assert.NotNull(result.Job);
        Assert.Equal("OK", result.Status);
    }

    [Theory]
    [InlineData("org1", "JOB1")]
    public void AddJobParametersNullTest(string orgId, string jobName)
    {
        var job = new MJob()
        {
            Name = jobName,
            Description = jobName
        };

        var repo = new Mock<IJobRepository>();
        repo.Setup(s => s.AddJob(job)).Returns(job);

        var redisHelper = new Mock<IRedisHelper>();
        var userRepo = new Mock<IUserRepository>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object, userRepo.Object);
        var result = jobSvc.AddJob(orgId, job);

        Assert.NotNull(result);
        Assert.NotNull(result.Job);
        Assert.Equal("OK", result.Status);
    }


    [Theory]
    [InlineData("org1", "JOB1", "aaa@email.com")]
    [InlineData("org1", "JOB1", "sss.email@gmail.com.xxx")]
    [InlineData("org1", "JOB1", "x@aaa.com")]
    public void AddJobValidateEmailOkTest(string orgId, string jobName, string email)
    {
        var job = new MJob()
        {
            Name = jobName,
            Description = jobName,
            Parameters = [new NameValue() { Name = "EMAIL_NOTI_ADDRESS", Value = email }]
        };

        var repo = new Mock<IJobRepository>();
        repo.Setup(s => s.AddJob(job)).Returns(job);

        var redisHelper = new Mock<IRedisHelper>();
        var userRepo = new Mock<IUserRepository>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object, userRepo.Object);
        var result = jobSvc.AddJob(orgId, job);

        Assert.NotNull(result);
        Assert.NotNull(result.Job);
        Assert.Equal("OK", result.Status);
    }
    //=====

    //===== AddJob() ====
    [Theory]
    [InlineData("org1", "", "ScanItemGenerator")]
    public void GetJobTemplateUserNameBlankTest(string orgId, string userName, string jobType)
    {
        var email = "your-email@email-xxx.com";

        var repo = new Mock<IJobRepository>();
        var redisHelper = new Mock<IRedisHelper>();
        var userRepo = new Mock<IUserRepository>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object, userRepo.Object);
        var result = jobSvc.GetJobTemplate(orgId, jobType, userName);

        var defaultEmail = "";
        foreach (var parm in result.Parameters)
        {
            if (parm.Name == "EMAIL_NOTI_ADDRESS")
            {
                defaultEmail = parm.Value;
                break;
            }
        }

        Assert.NotNull(result);
        Assert.Equal(email, defaultEmail);
    }

    [Theory]
    [InlineData("org1", "user1", "ScanItemGenerator")]
    public void GetJobTemplateUserNameInDbTest(string orgId, string userName, string jobType)
    {
        var email = "xxx@please-scan.com";
        var user = new MUser() { UserName = userName, UserEmail = email };

        var repo = new Mock<IJobRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(s => s.GetUserByName(userName)).Returns(user);

        var jobSvc = new JobService(repo.Object, redisHelper.Object, userRepo.Object);
        var result = jobSvc.GetJobTemplate(orgId, jobType, userName);

        var defaultEmail = "";
        foreach (var parm in result.Parameters)
        {
            if (parm.Name == "EMAIL_NOTI_ADDRESS")
            {
                defaultEmail = parm.Value;
                break;
            }
        }

        Assert.NotNull(result);
        Assert.Equal(email, defaultEmail);
    }
    //=====

    //===== DeleteJobById() ====
    [Theory]
    [InlineData("org1", "aaaa-bbbb-cccc-dddd")]
    public void DeleteJobByIdInvalidIdTest(string orgId, string jobId)
    {
        var repo = new Mock<IJobRepository>();
        var redisHelper = new Mock<IRedisHelper>();
        var userRepo = new Mock<IUserRepository>();

        var jobSvc = new JobService(repo.Object, redisHelper.Object, userRepo.Object);
        var result = jobSvc.DeleteJobById(orgId, jobId);

        Assert.NotNull(result);
        Assert.Equal("UUID_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    public void DeleteJobByIdNotFoundTest(string orgId, string jobId)
    {
        var repo = new Mock<IJobRepository>();
        repo.Setup(s => s.DeleteJobById(jobId)).Returns((MJob?)null);

        var redisHelper = new Mock<IRedisHelper>();
        var userRepo = new Mock<IUserRepository>();
        var jobSvc = new JobService(repo.Object, redisHelper.Object, userRepo.Object);

        var result = jobSvc.DeleteJobById(orgId, jobId);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }
    
    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    public void DeleteJobByIdOkTest(string orgId, string jobId)
    {
        var repo = new Mock<IJobRepository>();
        repo.Setup(s => s.DeleteJobById(jobId)).Returns(new MJob() {});

        var redisHelper = new Mock<IRedisHelper>();
        var userRepo = new Mock<IUserRepository>();
        var jobSvc = new JobService(repo.Object, redisHelper.Object, userRepo.Object);

        var result = jobSvc.DeleteJobById(orgId, jobId);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //=====
}
