using Moq;
using Its.Onix.Api.Services;
using Its.Onix.Api.Models;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;

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

        var apiKeySvc = new ApiKeyService(repo.Object);
        var result = await apiKeySvc.GetApiKey(orgId, apiKey);

        Assert.NotNull(result);
        Assert.Equal(apiKey, result.ApiKey);
    }
    //=====

    private MApiKey GetApiKey()
    {
        return null!;
    }

    //===== VerifyApiKey() ====
    [Theory]
    [InlineData("org1", "key1")]
    public void VerifyApiKeyNotFoundTest(string orgId, string apiKey)
    {
        MApiKey keyObj = new() { ApiKey = apiKey };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKey(apiKey)).ReturnsAsync(GetApiKey());

        var apiKeySvc = new ApiKeyService(repo.Object);
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

        var apiKeySvc = new ApiKeyService(repo.Object);

        apiKeySvc.SetCompareDate(DateTime.Now);
        var result = apiKeySvc.VerifyApiKey(orgId, apiKey);

        Assert.NotNull(result);
        Assert.Equal("EXPIRED", result.Status);
    }

    [Theory]
    [InlineData("org1", "key1")]
    public void VerifyApiKeyNotExpireNullTest(string orgId, string apiKey)
    {
        MApiKey keyObj = new() { ApiKey = apiKey, KeyExpiredDate = null };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKey(apiKey)).ReturnsAsync(keyObj);

        var apiKeySvc = new ApiKeyService(repo.Object);

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

        var apiKeySvc = new ApiKeyService(repo.Object);

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

        var apiKeySvc = new ApiKeyService(repo.Object);
        var result = apiKeySvc.AddApiKey(orgId, keyObj);

        Assert.NotNull(result);
        Assert.Equal("DUPLICATE", result.Status);
    }

    [Theory]
    [InlineData("org1", "key1")]
    public void AddApiKeyOkTest(string orgId, string apiKey)
    {
        MApiKey keyObj = new() { ApiKey = apiKey };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKey(apiKey)).ReturnsAsync(GetApiKey());

        var apiKeySvc = new ApiKeyService(repo.Object);
        var result = apiKeySvc.AddApiKey(orgId, keyObj);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //=====

    //===== DeleteApiKeyById() ====
    [Theory]
    [InlineData("org1", "id1")]
    public void DeleteApiKeyByIdInvalidTest(string orgId, string keyId)
    {
        var repo = new Mock<IApiKeyRepository>();

        var apiKeySvc = new ApiKeyService(repo.Object);
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

        var apiKeySvc = new ApiKeyService(repo.Object);
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

        var apiKeySvc = new ApiKeyService(repo.Object);
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

        var apiKeySvc = new ApiKeyService(repo.Object);
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

        var apiKeySvc = new ApiKeyService(repo.Object);
        var result = apiKeySvc.GetApiKeyCount(orgId, param);

        Assert.Equal(5, result);
    }
    //=====

    //===== GetApiKeyById() ====
    [Theory]
    [InlineData("org1", "xxxx")]
    public void GetApiKeyByIdOkTest(string orgId, string keyId)
    {
        var param = new MApiKey() { ApiKey = "XXXXX" };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.GetApiKeyById(keyId)).ReturnsAsync(param);

        var apiKeySvc = new ApiKeyService(repo.Object);
        var result = apiKeySvc.GetApiKeyById(orgId, keyId);

        Assert.IsType<MApiKey>(result);
        Assert.Equal("XXXXX", result.ApiKey);
    }
    //=====

    //===== UpdateApiKeyById() ====
    [Theory]
    [InlineData("org1", "xxxx")]
    public void UpdateApiKeyByIdNotFoundTest(string orgId, string keyId)
    {
        var param = new MApiKey() { ApiKey = "XXXXX" };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.UpdateApiKeyById(keyId, param)).Returns((MApiKey?)null);

        var apiKeySvc = new ApiKeyService(repo.Object);
        var result = apiKeySvc.UpdateApiKeyById(orgId, keyId, param);

        Assert.NotNull(result);
        Assert.Equal("NOTFOUND", result.Status);
    }

    [Theory]
    [InlineData("org1", "xxxx")]
    public void UpdateApiKeyByIdOkTest(string orgId, string keyId)
    {
        var param = new MApiKey() { ApiKey = "XXXXX" };

        var repo = new Mock<IApiKeyRepository>();
        repo.Setup(s => s.UpdateApiKeyById(keyId, param)).Returns(param);

        var apiKeySvc = new ApiKeyService(repo.Object);
        var result = apiKeySvc.UpdateApiKeyById(orgId, keyId, param);

        Assert.NotNull(result);
        Assert.Equal("OK", result.Status);
    }
    //=====
}