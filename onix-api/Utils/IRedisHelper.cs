
namespace Its.Onix.Api.Utils
{
    public interface IRedisHelper
    {
        public Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null);
        public Task SetObjectAsync<T>(string key, T obj, TimeSpan? expiry = null);
        public Task<T?> GetObjectAsync<T>(string key);
        public Task<string> PublishMessageAsync(string stream, string message);
        public Task<bool> DeleteAsync(string key);
    }
}
