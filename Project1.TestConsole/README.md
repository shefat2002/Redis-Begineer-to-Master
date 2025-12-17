# Project1.TestConsole

A minimal console application to test the Redis Key-Value Store API (Project1.KeyValueStoreAPI).

## Features

This console app tests all the main API endpoints:

1. **Create User** - Creates a new user profile
2. **Get User** - Retrieves a user by ID
3. **Update User** - Updates an existing user
4. **Check Exists** - Verifies if a user exists
5. **Get TTL** - Gets the Time-To-Live for a user key
6. **Create with TTL** - Creates a user with automatic expiration
7. **Delete User** - Removes a user
8. **Verify Deletion** - Confirms the user was deleted

## Prerequisites

- .NET 10.0 SDK
- Redis server running (via Docker or local installation)
- Project1.KeyValueStoreAPI running on http://localhost:5000

## Running the API First

Before running this test console, start the API:

```bash
# Start Redis
cd Project1.KeyValueStoreAPI
docker-compose up -d

# Run the API
dotnet run
```

The API should be running on http://localhost:5000

## Running the Test Console

```bash
cd Project1.TestConsole
dotnet run
```

## Expected Output

The console will display test results for each API operation:

```
=== Redis API Test Console ===

1. Creating a new user...
✓ User created: John Doe (ID: xxx-xxx-xxx)

2. Retrieving the user...
✓ User retrieved: John Doe
  Email: john.doe@example.com
  Phone: +1234567890

3. Updating the user...
✓ User updated: john.updated@example.com

4. Checking if user exists...
✓ User exists check: True

5. Getting user TTL...
✓ TTL: Key has no expiration (persistent)

6. Creating a user with 2 minute TTL...
✓ Temp user created with 2 min TTL: xxx-xxx-xxx

7. Deleting the first user...
✓ User deleted: xxx-xxx-xxx

8. Verifying user deletion...
✓ User confirmed deleted (404 Not Found)

=== All tests completed! ===
```

## Configuration

The console app connects to `http://localhost:5000` by default. To change this, modify the `baseUrl` constant in `Program.cs`.
