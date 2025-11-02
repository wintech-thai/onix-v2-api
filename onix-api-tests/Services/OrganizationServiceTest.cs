using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;
using System.Threading.Tasks;
using Google.Apis.Storage.v1.Data;

namespace Its.Onix.Api.Test.Services;

public class OrganizationServiceTest
{
    //===== IsOrgIdExist() ====
    [Theory]
    [InlineData("org1", true)]
    [InlineData("org2", false)]
    public void IsOrgIdExistTest(string orgId, bool isExist)
    {
        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        repo.Setup(s => s.IsCustomOrgIdExist(orgId)).Returns(isExist);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.IsOrgIdExist(orgId);

        Assert.Equal(isExist, result);
    }
    //====

    //===== AddOrganization() ====
    [Theory]
    [InlineData("org1")]
    public void AddOrganizationDuplicateTest(string orgId)
    {
        var org = new MOrganization()
        {
            OrgCustomId = orgId,
        };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        repo.Setup(s => s.IsCustomOrgIdExist(orgId)).Returns(true);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.AddOrganization(orgId, org);

        Assert.Equal("ORGANIZATION_DUPLICATE", result.Status);
    }

    [Theory]
    [InlineData("org1")]
    [InlineData("org2")]
    public void AddOrganizationOkTest(string orgId)
    {
        var org = new MOrganization()
        {
            OrgCustomId = orgId,
        };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        repo.Setup(s => s.IsCustomOrgIdExist(orgId)).Returns(false);
        repo.Setup(s => s.AddOrganization(org)).Returns(org);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.AddOrganization(orgId, org);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //====

    //===== GetOrganization() ====
    [Theory]
    [InlineData("org1")]
    public async Task GetOrganizationOkTest(string orgId)
    {
        var imagePath = "/xxx/ssss.logo.png";
        var presignedUrl = "https://this/is/presigned/url.jpg";

        var org = new MOrganization()
        {
            OrgCustomId = orgId,
            LogoImagePath = imagePath,
        };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        repo.Setup(s => s.GetOrganization()).ReturnsAsync(org);
        storageUtil.Setup(s => s.GenerateDownloadUrl(imagePath, It.IsAny<TimeSpan>(), "image/png")).Returns(presignedUrl);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = await orgSvc.GetOrganization(orgId);

        Assert.Equal(presignedUrl, result.LogoImageUrl);
    }
    //====

    //===== GetOrganization() ====
    [Theory]
    [InlineData("user1")]
    public void GetUserAllowedOrganizationTest(string username)
    {
        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        repo.Setup(s => s.GetUserAllowedOrganization(username)).Returns([]);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.GetUserAllowedOrganization(username);

        Assert.Empty(result);
    }
    //====

    //===== GetOrganization() ====
    [Theory]
    [InlineData("org1", "user1", true)]
    [InlineData("org1", "user1", false)]
    public void IsUserNameExistTest(string orgId, string username, bool isExist)
    {
        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        repo.Setup(s => s.IsUserNameExist(username)).Returns(isExist);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.IsUserNameExist(orgId, username);

        Assert.Equal(isExist, result);
    }
    //====

    //===== GetAllowAddressTypeNames() ====
    [Theory]
    [InlineData("org1")]
    public void GetAllowAddressTypeNamesTest(string orgId)
    {
        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.GetAllowAddressTypeNames(orgId);

        Assert.NotEmpty(result);
    }
    //====

    //===== GetAllowAddressTypeNames() ====
    [Theory]
    [InlineData("org1")]
    public void GetAllowChannelNamesTest(string orgId)
    {
        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.GetAllowChannelNames(orgId);

        Assert.NotEmpty(result);
    }
    //====

    //===== GetAllowAddressTypeNames() ====
    [Theory]
    [InlineData("org1")]
    public void GetLogoImageUploadPresignedUrl(string orgId)
    {
        var presignedUrl = "https://this/is/presigned/url.jpg";

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        storageUtil.Setup(s => s.GenerateDownloadUrl(It.IsAny<string>(), It.IsAny<TimeSpan>(), "image/png")).Returns(presignedUrl);
        storageUtil.Setup(s => s.GenerateUploadUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), "image/png")).Returns(presignedUrl);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.GetLogoImageUploadPresignedUrl(orgId);

        Assert.NotNull(result);
        Assert.Equal("SUCCESS", result.Status);
        Assert.Equal(presignedUrl, result.PreviewUrl);
    }
    //====

    //===== VerifyUserInOrganization() ====
    [Theory]
    [InlineData("org1", "user1")]
    public void VerifyUserInOrganizationNotFoundTest(string orgId, string userName)
    {
        MUser u = null!;

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        usrSvc.Setup(s => s.GetUserByName(orgId, userName)).Returns(u);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.VerifyUserInOrganization(orgId, userName);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "user1")]
    public void VerifyUserInOrganizationNotFoundInOrgTest(string orgId, string userName)
    {
        MUser u = new MUser();
        MOrganizationUser ou = null!;

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        usrSvc.Setup(s => s.GetUserByName(orgId, userName)).Returns(u);
        repo.Setup(s => s.GetUserInOrganization(userName)).Returns(ou);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.VerifyUserInOrganization(orgId, userName);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND_INORG", result.Status);
    }

    [Theory]
    [InlineData("org1", "user1")]
    public void VerifyUserInOrganizationNotActiveTest(string orgId, string userName)
    {
        MUser u = new MUser();
        MOrganizationUser ou = new MOrganizationUser() { UserStatus = "Disabled" };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        usrSvc.Setup(s => s.GetUserByName(orgId, userName)).Returns(u);
        repo.Setup(s => s.GetUserInOrganization(userName)).Returns(ou);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.VerifyUserInOrganization(orgId, userName);

        Assert.NotNull(result);
        Assert.Equal("NOT_ACTIVE_STATUS_USER", result.Status);
    }

    [Theory]
    [InlineData("org1", "user1")]
    public void VerifyUserInOrganizationOkTest(string orgId, string userName)
    {
        MUser u = new MUser();
        MOrganizationUser ou = new MOrganizationUser() { UserStatus = "Active" };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        usrSvc.Setup(s => s.GetUserByName(orgId, userName)).Returns(u);
        repo.Setup(s => s.GetUserInOrganization(userName)).Returns(ou);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.VerifyUserInOrganization(orgId, userName);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //====

    //===== AddUserToOrganization() ====
    [Theory]
    [InlineData("org1", "user1")]
    public void AddUserToOrganizationUserNameNotFoundTest(string orgId, string userName)
    {
        MOrganizationUser ou = new MOrganizationUser() { UserName = userName };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        usrSvc.Setup(s => s.IsUserNameExist(orgId, userName)).Returns(false);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.AddUserToOrganization(orgId, ou);

        Assert.NotNull(result);
        Assert.Equal("USER_NAME_NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "user1")]
    public void AddUserToOrganizationUserIdNotFoundTest(string orgId, string userName)
    {
        MOrganizationUser ou = new() { UserName = userName, UserId = Guid.NewGuid().ToString() };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        usrSvc.Setup(s => s.IsUserNameExist(orgId, userName)).Returns(true);
        usrSvc.Setup(s => s.IsUserIdExist(orgId, userName)).Returns(false);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.AddUserToOrganization(orgId, ou);

        Assert.NotNull(result);
        Assert.Equal("USER_ID_NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "user1")]
    public void AddUserToOrganizationUserDuplicateTest(string orgId, string userName)
    {
        MOrganizationUser ou = new() { UserName = userName, UserId = Guid.NewGuid().ToString() };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        usrSvc.Setup(s => s.IsUserNameExist(orgId, userName)).Returns(true);
        usrSvc.Setup(s => s.IsUserIdExist(orgId, ou.UserId)).Returns(true);
        repo.Setup(s => s.IsUserNameExist(userName)).Returns(true);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.AddUserToOrganization(orgId, ou);

        Assert.NotNull(result);
        Assert.Equal("USER_DUPLICATE", result.Status);
    }

    [Theory]
    [InlineData("org1", "user1")]
    public void AddUserToOrganizationOkTest(string orgId, string userName)
    {
        MOrganizationUser ou = new() { UserName = userName, UserId = Guid.NewGuid().ToString() };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        usrSvc.Setup(s => s.IsUserNameExist(orgId, userName)).Returns(true);
        usrSvc.Setup(s => s.IsUserIdExist(orgId, ou.UserId)).Returns(true);
        repo.Setup(s => s.IsUserNameExist(userName)).Returns(false);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = orgSvc.AddUserToOrganization(orgId, ou);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //====

    //===== UpdateOrganization() ====
    [Theory]
    [InlineData("org1")]
    public async Task UpdateOrganizationObjectNotFoundTest(string orgId)
    {
        var imagePath = "/xxx/ssss.logo.png";

        var org = new MOrganization()
        {
            OrgCustomId = orgId,
            LogoImagePath = imagePath,
            ChannelsArray = null!,
            AddressesArray = null!,
        };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        repo.Setup(s => s.GetOrganization()).ReturnsAsync(org);
        storageUtil.Setup(s => s.IsObjectExist(imagePath)).Returns(false);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = await orgSvc.UpdateOrganization(orgId, org);

        Assert.Equal("OBJECT_NOT_FOUND", result.Status);
    }

    [Theory]
    [InlineData("org1")]
    public async Task UpdateOrganizationValidateFailTest(string orgId)
    {
        var imagePath = "/xxx/ssss.logo.png";
        Google.Apis.Storage.v1.Data.Object storageObj = null!;
        /*
                new()
                {
                    Size = 2 * 1024 * 1024,
                    ContentType = "image/png"
                };
        */

        var org = new MOrganization()
        {
            OrgCustomId = orgId,
            LogoImagePath = imagePath,
            ChannelsArray = null!,
            AddressesArray = null!,
        };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        repo.Setup(s => s.GetOrganization()).ReturnsAsync(org);
        storageUtil.Setup(s => s.IsObjectExist(imagePath)).Returns(true);
        storageUtil.Setup(s => s.GetStorageObject(It.IsAny<string>(), imagePath)).Returns(storageObj);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = await orgSvc.UpdateOrganization(orgId, org);

        Assert.Equal("OBJECT_NOT_FOUND", result.Status);
    }

    [Theory]
    [InlineData("org1")]
    public async Task UpdateOrganizationTooBigTest(string orgId)
    {
        var imagePath = "/xxx/ssss.logo.png";
        Google.Apis.Storage.v1.Data.Object storageObj = new()
        {
            Size = 2 * 1024 * 1024,
            ContentType = "image/png"
        };

        var org = new MOrganization()
        {
            OrgCustomId = orgId,
            LogoImagePath = imagePath,
            ChannelsArray = null!,
            AddressesArray = null!,
        };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        repo.Setup(s => s.GetOrganization()).ReturnsAsync(org);
        storageUtil.Setup(s => s.IsObjectExist(imagePath)).Returns(true);
        storageUtil.Setup(s => s.GetStorageObject(It.IsAny<string>(), imagePath)).Returns(storageObj);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = await orgSvc.UpdateOrganization(orgId, org);

        Assert.Equal("FILE_TOO_BIG", result.Status);
    }

    [Theory]
    [InlineData("org1")]
    public async Task UpdateOrganizationInvalidFileTypeTest(string orgId)
    {
        var imagePath = "/xxx/ssss.logo.png";
        Google.Apis.Storage.v1.Data.Object storageObj = new()
        {
            Size = 1 * 1024 * 1024,
            ContentType = "image/xxx"
        };

        var org = new MOrganization()
        {
            OrgCustomId = orgId,
            LogoImagePath = imagePath,
            ChannelsArray = null!,
            AddressesArray = null!,
        };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        repo.Setup(s => s.GetOrganization()).ReturnsAsync(org);
        storageUtil.Setup(s => s.IsObjectExist(imagePath)).Returns(true);
        storageUtil.Setup(s => s.GetStorageObject(It.IsAny<string>(), imagePath)).Returns(storageObj);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = await orgSvc.UpdateOrganization(orgId, org);

        Assert.Equal("FILE_TYPE_NOT_PNG", result.Status);
    }

    [Theory]
    [InlineData("org1")]
    public async Task UpdateOrganizationInvalidPngTest(string orgId)
    {
        var header = new byte[10];
        var imagePath = "/xxx/ssss.logo.png";
        Google.Apis.Storage.v1.Data.Object storageObj = new()
        {
            Size = 1000,
            ContentType = "image/png"
        };

        var org = new MOrganization()
        {
            OrgCustomId = orgId,
            LogoImagePath = imagePath,
            ChannelsArray = null!,
            AddressesArray = null!,
        };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        repo.Setup(s => s.GetOrganization()).ReturnsAsync(org);
        storageUtil.Setup(s => s.IsObjectExist(imagePath)).Returns(true);
        storageUtil.Setup(s => s.GetStorageObject(It.IsAny<string>(), imagePath)).Returns(storageObj);
        storageUtil.Setup(s => s.PartialDownloadToStream(It.IsAny<string>(), imagePath!, 0, 24)).ReturnsAsync(header);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = await orgSvc.UpdateOrganization(orgId, org);

        Assert.Equal("NOT_VALID_PNG_FILE", result.Status);
    }

    [Theory]
    [InlineData("org1")]
    public async Task UpdateOrganizationInvalidDimentionTest(string orgId)
    {
        var header = new byte[24];
        // 1500 decimal
        header[16] = 0x00;
        header[17] = 0x00;
        header[18] = 0x05;
        header[19] = 0xDC;

        // 1500 decimal
        header[20] = 0x00;
        header[21] = 0x00;
        header[22] = 0x05;
        header[23] = 0xDC;

        var imagePath = "/xxx/ssss.logo.png";
        Google.Apis.Storage.v1.Data.Object storageObj = new()
        {
            Size = 1000,
            ContentType = "image/png"
        };

        var org = new MOrganization()
        {
            OrgCustomId = orgId,
            LogoImagePath = imagePath,
            ChannelsArray = null!,
            AddressesArray = null!,
        };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        repo.Setup(s => s.GetOrganization()).ReturnsAsync(org);
        storageUtil.Setup(s => s.IsObjectExist(imagePath)).Returns(true);
        storageUtil.Setup(s => s.GetStorageObject(It.IsAny<string>(), imagePath)).Returns(storageObj);
        storageUtil.Setup(s => s.PartialDownloadToStream(It.IsAny<string>(), imagePath!, 0, 24)).ReturnsAsync(header);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = await orgSvc.UpdateOrganization(orgId, org);

        Assert.Equal("INVALID_IMAGE_DIMENSION", result.Status);
    }
    
    [Theory]
    [InlineData("org1")]
    public async Task UpdateOrganizationOkTest(string orgId)
    {
        var header = new byte[24];
        // ตั้งค่าให้ offset 16 = 100
        header[16] = 0x00;
        header[17] = 0x00;
        header[18] = 0x00;
        header[19] = 0x64; // 100 decimal

        // ตั้งค่าให้ offset 20 = 100
        header[20] = 0x00;
        header[21] = 0x00;
        header[22] = 0x00;
        header[23] = 0x64; // 100 decimal

        var imagePath = "/xxx/ssss.logo.png";
        Google.Apis.Storage.v1.Data.Object storageObj = new()
        {
            Size = 1000,
            ContentType = "image/png"
        };

        var org = new MOrganization()
        {
            OrgCustomId = orgId,
            LogoImagePath = imagePath,
            ChannelsArray = null!,
            AddressesArray = null!,
        };

        var repo = new Mock<IOrganizationRepository>();
        var usrSvc = new Mock<IUserService>();
        var storageUtil = new Mock<IStorageUtils>();

        repo.Setup(s => s.GetOrganization()).ReturnsAsync(org);
        storageUtil.Setup(s => s.IsObjectExist(imagePath)).Returns(true);
        storageUtil.Setup(s => s.GetStorageObject(It.IsAny<string>(), imagePath)).Returns(storageObj);
        storageUtil.Setup(s => s.PartialDownloadToStream(It.IsAny<string>(), imagePath!, 0, 24)).ReturnsAsync(header);

        var orgSvc = new OrganizationService(repo.Object, usrSvc.Object, storageUtil.Object);
        var result = await orgSvc.UpdateOrganization(orgId, org);

        Assert.Equal("OK", result.Status);
    }
    //====
}