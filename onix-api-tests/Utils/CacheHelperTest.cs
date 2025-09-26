
using Its.Onix.Api.Utils;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Its.Onix.Api.Test.Utils;

public class CacheHelperTest
{
    [Theory]
    [InlineData("orgid1", "ThisisApiName", "orgid1:Local:ThisisApiName")]
    public void CreateApiOtpKeyTest(string orgId, string apiName, string expectKey)
    {
        var key = CacheHelper.CreateApiOtpKey(orgId, apiName);
        Assert.Equal(expectKey, key);
    }
}