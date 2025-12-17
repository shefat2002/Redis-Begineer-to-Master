# Project 1: Simple Key-Value Store API

A Redis-backed RESTful API for managing user profiles with TTL support.

## Quick Start

### 1. Start Redis
```bash
cd Project1.KeyValueStoreAPI
docker-compose up -d
```

### 2. Run the API
```bash
dotnet run
```

### 3. Test the API
```bash
# Create a user
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "dateOfBirth": "1990-01-01",
    "phoneNumber": "+1234567890",
    "address": "123 Main St"
  }'

# Get user (replace {id} with returned ID)
curl http://localhost:5000/api/users/{id}
```

## Features

- ✅ Create user profiles with optional TTL
- ✅ Retrieve user by ID
- ✅ Update existing users
- ✅ Delete users
- ✅ Check remaining TTL
- ✅ Redis persistence with AOF
- ✅ Redis Commander GUI on port 8081

## Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/users` | Create new user |
| GET | `/api/users/{id}` | Get user by ID |
| PUT | `/api/users/{id}` | Update user |
| DELETE | `/api/users/{id}` | Delete user |
| GET | `/api/users/{id}/ttl` | Check TTL |
| GET | `/api/users/{id}/exists` | Check existence |

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- StackExchange.Redis 2.8.16
- Redis 7 (Docker)
- Redis Commander (GUI)

## Documentation

See [PROJECT1-IMPLEMENTATION.md](../PROJECT1-IMPLEMENTATION.md) for comprehensive documentation including:
- Architecture explanation
- Code walkthrough
- Redis concepts
- Testing guide
- Troubleshooting
- Learning exercises

## Learning Objectives

- Redis string operations (SET, GET, DEL)
- ConnectionMultiplexer pattern
- TTL management
- JSON serialization
- Service layer architecture
