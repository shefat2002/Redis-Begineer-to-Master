using System.Net.Http.Json;
using Project1.TestConsole;

const string baseUrl = "http://localhost:6379/";
using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

Console.WriteLine("=== Redis API Test Console ===\n");

try
{
    // Test 1: Create a user
    Console.WriteLine("1. Creating a new user...");
    var newUser = new UserProfile
    {
        Id = Guid.NewGuid().ToString(),
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        DateOfBirth = new DateTime(1990, 5, 15),
        PhoneNumber = "+1234567890",
        Address = "123 Main St, City, Country",
        CreatedAt = DateTime.UtcNow
    };

    var createResponse = await httpClient.PostAsJsonAsync("/api/users", newUser);
    if (createResponse.IsSuccessStatusCode)
    {
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserProfile>();
        Console.WriteLine($"✓ User created: {createdUser?.FirstName} {createdUser?.LastName} (ID: {createdUser?.Id})");
    }
    else
    {
        Console.WriteLine($"✗ Failed to create user: {createResponse.StatusCode}");
    }

    // Test 2: Get the user
    Console.WriteLine("\n2. Retrieving the user...");
    var getResponse = await httpClient.GetAsync($"/api/users/{newUser.Id}");
    if (getResponse.IsSuccessStatusCode)
    {
        var retrievedUser = await getResponse.Content.ReadFromJsonAsync<UserProfile>();
        Console.WriteLine($"✓ User retrieved: {retrievedUser?.FirstName} {retrievedUser?.LastName}");
        Console.WriteLine($"  Email: {retrievedUser?.Email}");
        Console.WriteLine($"  Phone: {retrievedUser?.PhoneNumber}");
    }
    else
    {
        Console.WriteLine($"✗ Failed to retrieve user: {getResponse.StatusCode}");
    }

    // Test 3: Update the user
    Console.WriteLine("\n3. Updating the user...");
    newUser.Email = "john.updated@example.com";
    newUser.UpdatedAt = DateTime.UtcNow;
    var updateResponse = await httpClient.PutAsJsonAsync($"/api/users/{newUser.Id}", newUser);
    if (updateResponse.IsSuccessStatusCode)
    {
        var updatedUser = await updateResponse.Content.ReadFromJsonAsync<UserProfile>();
        Console.WriteLine($"✓ User updated: {updatedUser?.Email}");
    }
    else
    {
        Console.WriteLine($"✗ Failed to update user: {updateResponse.StatusCode}");
    }

    // Test 4: Check if user exists
    Console.WriteLine("\n4. Checking if user exists...");
    var existsResponse = await httpClient.GetAsync($"/api/users/{newUser.Id}/exists");
    if (existsResponse.IsSuccessStatusCode)
    {
        var existsResult = await existsResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Console.WriteLine($"✓ User exists check: {existsResult?["exists"]}");
    }
    else
    {
        Console.WriteLine($"✗ Failed to check existence: {existsResponse.StatusCode}");
    }

    // Test 5: Get TTL
    Console.WriteLine("\n5. Getting user TTL...");
    var ttlResponse = await httpClient.GetAsync($"/api/users/{newUser.Id}/ttl");
    if (ttlResponse.IsSuccessStatusCode)
    {
        var ttlResult = await ttlResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Console.WriteLine($"✓ TTL: {ttlResult?["message"]}");
    }
    else
    {
        Console.WriteLine($"✗ Failed to get TTL: {ttlResponse.StatusCode}");
    }

    // Test 6: Create user with TTL
    Console.WriteLine("\n6. Creating a user with 2 minute TTL...");
    var tempUser = new UserProfile
    {
        Id = Guid.NewGuid().ToString(),
        FirstName = "Jane",
        LastName = "Smith",
        Email = "jane.smith@example.com",
        DateOfBirth = new DateTime(1995, 8, 20),
        PhoneNumber = "+9876543210",
        Address = "456 Oak Ave, Town, Country",
        CreatedAt = DateTime.UtcNow
    };
    var createTtlResponse = await httpClient.PostAsJsonAsync("/api/users?ttlMinutes=2", tempUser);
    if (createTtlResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"✓ Temp user created with 2 min TTL: {tempUser.Id}");
    }

    // Test 7: Delete the first user
    Console.WriteLine("\n7. Deleting the first user...");
    var deleteResponse = await httpClient.DeleteAsync($"/api/users/{newUser.Id}");
    if (deleteResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"✓ User deleted: {newUser.Id}");
    }
    else
    {
        Console.WriteLine($"✗ Failed to delete user: {deleteResponse.StatusCode}");
    }

    // Test 8: Verify deletion
    Console.WriteLine("\n8. Verifying user deletion...");
    var verifyResponse = await httpClient.GetAsync($"/api/users/{newUser.Id}");
    if (verifyResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        Console.WriteLine("✓ User confirmed deleted (404 Not Found)");
    }
    else
    {
        Console.WriteLine($"✗ Unexpected result: {verifyResponse.StatusCode}");
    }

    Console.WriteLine("\n=== All tests completed! ===");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"\n✗ Connection error: {ex.Message}");
    Console.WriteLine("Make sure the API is running on http://localhost:5000");
}
catch (Exception ex)
{
    Console.WriteLine($"\n✗ Error: {ex.Message}");
}
