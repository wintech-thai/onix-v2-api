using StackExchange.Redis;

namespace Its.Onix.Api.Utils
{
    public class RedisHelper
    {
        private readonly IDatabase _db;

        public RedisHelper(IConnectionMultiplexer connection)
        {
            _db = connection.GetDatabase();
        }

        public Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
            => _db.StringSetAsync(key, value, expiry);

        public async Task<string?> GetAsync(string key)
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }

        public Task<bool> DeleteAsync(string key)
            => _db.KeyDeleteAsync(key);
    }
}
