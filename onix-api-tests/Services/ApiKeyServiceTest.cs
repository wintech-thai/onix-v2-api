using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Test.Services;

public class ApiKeyServiceTest
{
    //===== GetApiKey() ====
    [Theory]
    [InlineData("org1", "key1")]
    public async Task GetApiKeyOkTest(string orgId, string apiKey)
    {
        var keyObj = new MApiKey() { ApiKey = apiKey };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKey(apiKey)).ReturnsAsync(keyObj);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = await apiKeySvc.GetApiKey(orgId, apiKey);

        Assert.NotNull(result);
        Assert.Equal(apiKey, result.ApiKey);
    }
    //=====

    //===== GetApiKeyByName() ====
    [Theory]
    [InlineData("org1", "name1")]
    public async Task GetApiKeyByNameOkTest(string orgId, string keyName)
    {
        var keyObj = new MApiKey() { KeyName = keyName, ApiKey = "xxxxx" };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKeyByName(keyName)).ReturnsAsync(keyObj);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = await apiKeySvc.GetApiKeyByName(orgId, keyName);

        Assert.NotNull(result);
        Assert.Equal("xxxxx", result.ApiKey);
    }
    //=====

    private MApiKey GetApiKey()
    {
        return null!;
    }

    //===== VerifyApiKey() ====
    [Theory]
    [InlineData("org1", "key1", "name1")]
    public void VerifyApiKeyNotFoundTest(string orgId, string apiKey, string keyName)
    {
        MApiKey keyObj = new() { ApiKey = apiKey, KeyName = keyName };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKey(apiKey)).ReturnsAsync(GetApiKey());
        repo.Setup(s => s.GetApiKeyByName(keyName)).ReturnsAsync(GetApiKey());

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.VerifyApiKey(orgId, apiKey);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "key1")]
    public void VerifyApiKeyExpireTest(string orgId, string apiKey)
    {
        MApiKey keyObj = new() { ApiKey = apiKey, KeyExpiredDate = DateTime.Now.AddDays(-1) };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKey(apiKey)).ReturnsAsync(keyObj);

        var redisHelper = new Mock<IRedisHelper>();
        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);

        apiKeySvc.SetCompareDate(DateTime.Now);
        var result = apiKeySvc.VerifyApiKey(orgId, apiKey);

        Assert.NotNull(result);
        Assert.Equal("EXPIRED", result.Status);
    }

    [Theory]
    [InlineData("org1", "key1")]
    public void VerifyApiKeyDisableTest(string orgId, string apiKey)
    {
        MApiKey keyObj = new() { ApiKey = apiKey, KeyExpiredDate = DateTime.Now.AddDays(1), KeyStatus = "Disabled" };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKey(apiKey)).ReturnsAsync(keyObj);

        var redisHelper = new Mock<IRedisHelper>();
        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);

        apiKeySvc.SetCompareDate(DateTime.Now);
        var result = apiKeySvc.VerifyApiKey(orgId, apiKey);

        Assert.NotNull(result);
        Assert.Equal("DISABLED", result.Status);
    }

    [Theory]
    [InlineData("org1", "key1")]
    public void VerifyApiKeyNotExpireNullTest(string orgId, string apiKey)
    {
        MApiKey keyObj = new() { ApiKey = apiKey, KeyExpiredDate = null };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKey(apiKey)).ReturnsAsync(keyObj);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);

        apiKeySvc.SetCompareDate(DateTime.Now);
        var result = apiKeySvc.VerifyApiKey(orgId, apiKey);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }

    [Theory]
    [InlineData("org1", "key1")]
    public void VerifyApiKeyNotExpireTest(string orgId, string apiKey)
    {
        MApiKey keyObj = new() { ApiKey = apiKey, KeyExpiredDate = DateTime.Now.AddDays(1) };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKey(apiKey)).ReturnsAsync(keyObj);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);

        apiKeySvc.SetCompareDate(DateTime.Now);
        var result = apiKeySvc.VerifyApiKey(orgId, apiKey);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //=====

    //===== VerifyApiKey() ====
    [Theory]
    [InlineData("org1", "key1")]
    public void AddApiKeyDuplicateTest(string orgId, string apiKey)
    {
        MApiKey keyObj = new() { ApiKey = apiKey };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKey(apiKey)).ReturnsAsync(keyObj);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.AddApiKey(orgId, keyObj);

        Assert.NotNull(result);
        Assert.Equal("KEY_DUPLICATE", result.Status);
    }

    //===== VerifyApiKey() ====
    [Theory]
    [InlineData("org1", "key1", "name1")]
    public void AddApiKeyNameDuplicateTest(string orgId, string apiKey, string keyName)
    {
        MApiKey keyObj = new() { ApiKey = apiKey, KeyName = keyName };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKey(apiKey)).ReturnsAsync((MApiKey) null!);
        repo.Setup(s => s.GetApiKeyByName(keyName)).ReturnsAsync(keyObj);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.AddApiKey(orgId, keyObj);

        Assert.NotNull(result);
        Assert.Equal("NAME_DUPLICATE", result.Status);
    }

    [Theory]
    [InlineData("org1", "key1", "name1", "")]
    [InlineData("org1", "key1", "name1", "A,B")]
    public void AddApiKeyOkTest(string orgId, string apiKey, string keyName, string roleList)
    {
        var roles = roleList.Split(",").ToList();
        if (roleList == "")
        {
            roles = null;
        }

        MApiKey keyObj = new() { ApiKey = apiKey, KeyName = keyName, Roles = roles! };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKey(apiKey)).ReturnsAsync(GetApiKey());
        repo.Setup(s => s.GetApiKeyByName(keyName)).ReturnsAsync(GetApiKey());
        repo.Setup(s => s.AddApiKey(keyObj)).Returns(keyObj);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.AddApiKey(orgId, keyObj);

        Assert.NotNull(result);
        Assert.NotNull(result.ApiKey);

        Assert.Equal("OK", result.Status);
        Assert.Equal("Active", result.ApiKey.KeyStatus);
    }
    //=====

    //===== DeleteApiKeyById() ====
    [Theory]
    [InlineData("org1", "id1")]
    public void DeleteApiKeyByIdInvalidTest(string orgId, string keyId)
    {
        var repo = new Mock<IApiKeyRepository>();
        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.DeleteApiKeyById(orgId, keyId);

        Assert.NotNull(result);
        Assert.Equal("UUID_INVALID", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    public void DeleteApiKeyByIdNotFoundTest(string orgId, string keyId)
    {
        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.DeleteApiKeyById(keyId)).Returns(GetApiKey());

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.DeleteApiKeyById(orgId, keyId);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
    public void DeleteApiKeyByIdOkTest(string orgId, string keyId)
    {
        MApiKey keyObj = new() { KeyId = Guid.Parse(keyId) };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.DeleteApiKeyById(keyId)).Returns(keyObj);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.DeleteApiKeyById(orgId, keyId);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //=====

    //===== GetApiKeys() ====
    [Theory]
    [InlineData("org1", "xxxx")]
    public void GetApiKeysOkTest(string orgId, string textSearch)
    {
        var param = new VMApiKey() { FullTextSearch = textSearch };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKeys(param)).Returns([
            new MApiKey(),
            new MApiKey()
        ]);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.GetApiKeys(orgId, param);

        Assert.NotNull(result);
        Assert.Equal(2, result.ToArray().Length);
    }
    //=====

    //===== GetApiKeyCount() ====
    [Theory]
    [InlineData("org1", "xxxx")]
    public void GetApiKeyCountOkTest(string orgId, string textSearch)
    {
        var param = new VMApiKey() { FullTextSearch = textSearch };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKeyCount(param)).Returns(5);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.GetApiKeyCount(orgId, param);

        Assert.Equal(5, result);
    }
    //=====

    //===== GetApiKeyById() ====
    [Theory]
    [InlineData("org1", "xxxx")]
    public void GetApiKeyByIdOkTest(string orgId, string keyId)
    {
        var param = new MApiKey() { ApiKey = "XXXXX", RolesList = "A,B" };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKeyById(keyId)).ReturnsAsync(param);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.GetApiKeyById(orgId, keyId);

        Assert.IsType<MApiKey>(result);
        Assert.Equal("", result.ApiKey);
    }

    [Theory]
    [InlineData("org1", "xxxx")]
    public void GetApiKeyByIdNotFoundTest(string orgId, string keyId)
    {
        var param = new MApiKey() { ApiKey = "XXXXX" };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKeyById(keyId)).ReturnsAsync((MApiKey) null!);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.GetApiKeyById(orgId, keyId);

        Assert.Null(result);
    }
    //=====

    //===== UpdateApiKeyStatusById() ====
    [Theory]
    [InlineData("org1", "xxxx", "Active")]
    public void UpdateApiKeyStatusByIdNotFoundTest(string orgId, string keyId, string status)
    {
        var param = new MApiKey() { ApiKey = "XXXXX" };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.UpdateApiKeyStatusById(keyId, status)).Returns((MApiKey?)null);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.UpdateApiKeyStatusById(orgId, keyId, status);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }
    
    [Theory]
    [InlineData("org1", "xxxx", "Active")]
    public void UpdateApiKeyStatusByIdOkTest(string orgId, string keyId, string status)
    {
        var keyObj = new MApiKey() { ApiKey = "XXXXX" };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.UpdateApiKeyStatusById(keyId, status)).Returns(keyObj);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.UpdateApiKeyStatusById(orgId, keyId, status);

        Assert.NotNull(result);
        Assert.NotNull(result.ApiKey);

        Assert.Equal("OK", result.Status);
        Assert.Equal("", result.ApiKey.ApiKey);
        Assert.Equal("", result.ApiKey.RolesList);
    }
    //===

    //===== UpdateApiKeyById() ====
    [Theory]
    [InlineData("org1", "xxxx")]
    public void UpdateApiKeyByIdNotFoundTest(string orgId, string keyId)
    {
        var param = new MApiKey() { ApiKey = "XXXXX" };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.UpdateApiKeyById(keyId, param)).Returns((MApiKey?)null);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.UpdateApiKeyById(orgId, keyId, param);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "xxxx", "")]
    [InlineData("org1", "xxxx", "A,B,C")]
    public void UpdateApiKeyByIdOkTest(string orgId, string keyId, string roleList)
    {
        var roles = roleList.Split(",").ToList();
        if (roleList == "")
        {
            roles = null;
        }

        var param = new MApiKey() { ApiKey = "XXXXX", Roles = roles! };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.UpdateApiKeyById(keyId, param)).Returns(param);

        var redisHelper = new Mock<IRedisHelper>();

        var apiKeySvc = new ApiKeyService(repo.Object, redisHelper.Object);
        var result = apiKeySvc.UpdateApiKeyById(orgId, keyId, param);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //=====
}