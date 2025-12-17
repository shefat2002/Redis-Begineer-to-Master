using Microsoft.AspNetCore.Mvc;
using Project1.KeyValueStoreAPI.Models;
using Project1.KeyValueStoreAPI.Services;

namespace Project1.KeyValueStoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IRedisService _redisService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IRedisService redisService, ILogger<UsersController> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new user profile
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserProfile user, [FromQuery] int? ttlMinutes = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(user.Id))
            {
                user.Id = Guid.NewGuid().ToString();
            }

            TimeSpan? expiry = ttlMinutes.HasValue ? TimeSpan.FromMinutes(ttlMinutes.Value) : null;
            
            var created = await _redisService.CreateUserAsync(user, expiry);
            
            if (!created)
            {
                return Conflict(new { message = $"User with ID {user.Id} already exists" });
            }

            _logger.LogInformation("Created user {UserId} with TTL: {TTL}", user.Id, expiry?.ToString() ?? "None");
            
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get user profile by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        try
        {
            var user = await _redisService.GetUserAsync(id);
            
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update an existing user profile
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserProfile user, [FromQuery] int? ttlMinutes = null)
    {
        try
        {
            if (id != user.Id)
            {
                return BadRequest(new { message = "ID in URL does not match ID in body" });
            }

            TimeSpan? expiry = ttlMinutes.HasValue ? TimeSpan.FromMinutes(ttlMinutes.Value) : null;
            
            var updated = await _redisService.UpdateUserAsync(user, expiry);
            
            if (!updated)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }

            _logger.LogInformation("Updated user {UserId}", id);
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete a user profile
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            var deleted = await _redisService.DeleteUserAsync(id);
            
            if (!deleted)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }

            _logger.LogInformation("Deleted user {UserId}", id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get the remaining TTL (Time To Live) for a user
    /// </summary>
    [HttpGet("{id}/ttl")]
    public async Task<IActionResult> GetUserTtl(string id)
    {
        try
        {
            var exists = await _redisService.UserExistsAsync(id);
            
            if (!exists)
            {
                return NotFound(new { message = $"User with ID {id} not found" });
            }

            var ttl = await _redisService.GetTtlAsync(id);
            
            return Ok(new
            {
                userId = id,
                ttl = ttl?.TotalSeconds ?? -1,
                message = ttl.HasValue 
                    ? $"Key expires in {ttl.Value.TotalSeconds:F0} seconds" 
                    : "Key has no expiration (persistent)"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TTL for user {UserId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Check if a user exists
    /// </summary>
    [HttpGet("{id}/exists")]
    public async Task<IActionResult> CheckUserExists(string id)
    {
        try
        {
            var exists = await _redisService.UserExistsAsync(id);
            return Ok(new { userId = id, exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} exists", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
