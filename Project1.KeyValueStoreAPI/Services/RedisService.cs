using System.Text.Json;
using Project1.KeyValueStoreAPI.Models;
using StackExchange.Redis;

namespace Project1.KeyValueStoreAPI.Services;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly string _keyPrefix = "user:";

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    private string GetKey(string userId) => $"{_keyPrefix}{userId}";

    public async Task<bool> CreateUserAsync(UserProfile user, TimeSpan? expiry = null)
    {
        var key = GetKey(user.Id);
        var json = JsonSerializer.Serialize(user);
        
        // Using SET with NX (Not eXists) option to ensure we don't overwrite existing user
        var created = await _db.StringSetAsync(key, json, expiry, When.NotExists);
        return created;
    }

    public async Task<UserProfile?> GetUserAsync(string userId)
    {
        var key = GetKey(userId);
        var value = await _db.StringGetAsync(key);
        
        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<UserProfile>(value.ToString());
    }

    public async Task<bool> UpdateUserAsync(UserProfile user, TimeSpan? expiry = null)
    {
        var key = GetKey(user.Id);
        
        // Check if user exists
        if (!await _db.KeyExistsAsync(key))
            return false;

        user.UpdatedAt = DateTime.UtcNow;
        var json = JsonSerializer.Serialize(user);
        
        // Using SET with XX (eXists) option to ensure we only update existing users
        await _db.StringSetAsync(key, json, expiry, When.Exists);
        return true;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var key = GetKey(userId);
        return await _db.KeyDeleteAsync(key);
    }

    public async Task<TimeSpan?> GetTtlAsync(string userId)
    {
        var key = GetKey(userId);
        return await _db.KeyTimeToLiveAsync(key);
    }

    public async Task<bool> UserExistsAsync(string userId)
    {
        var key = GetKey(userId);
        return await _db.KeyExistsAsync(key);
    }
}
