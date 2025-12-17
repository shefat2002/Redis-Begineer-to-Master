using Project1.KeyValueStoreAPI.Models;

namespace Project1.KeyValueStoreAPI.Services;

public interface IRedisService
{
    Task<bool> CreateUserAsync(UserProfile user, TimeSpan? expiry = null);
    Task<UserProfile?> GetUserAsync(string userId);
    Task<bool> UpdateUserAsync(UserProfile user, TimeSpan? expiry = null);
    Task<bool> DeleteUserAsync(string userId);
    Task<TimeSpan?> GetTtlAsync(string userId);
    Task<bool> UserExistsAsync(string userId);
}
