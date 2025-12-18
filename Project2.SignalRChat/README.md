# Project 2: SignalR Chat with Redis Backplane

A real-time chat application built with ASP.NET Core SignalR and Redis as a backplane for horizontal scaling.

## 🎯 Features

- **Real-time messaging** using SignalR WebSockets
- **Redis backplane** for scaling across multiple server instances
- **Group chat support** for room-based conversations
- **Connection management** with automatic reconnection
- **Modern UI** with gradient design
- **System notifications** for user joins/leaves
- **Cross-origin support** for client applications

## 🏗️ Architecture

```
Client (Browser)
    ↓ WebSocket
SignalR Hub (ChatHub)
    ↓
Redis Backplane
    ↓
Multiple Server Instances (Scalable)
```

## 📦 Technologies Used

- **ASP.NET Core 10.0** - Web framework
- **SignalR** - Real-time communication
- **Redis** - Message backplane for scaling
- **StackExchange.Redis** - Redis client
- **HTML/CSS/JavaScript** - Frontend

## 🚀 Getting Started

### Prerequisites

1. .NET 10.0 SDK or later
2. Docker (optional, for Redis + Redis Commander)

### Option 1: Quick Start with Docker (Recommended) 🐳

```bash
# Start Redis + Redis Commander in Docker
./docker-start.sh

# Run the app locally
dotnet run

# Open browser
# Chat: http://localhost:5000
# Redis Commander: http://localhost:8081
```

See [DOCKER.md](DOCKER.md) for detailed Docker instructions and Redis Commander usage!

### Option 2: Manual Redis Setup

```bash
# Using Docker
docker run -d -p 6379:6379 redis:latest

# Or using Homebrew on macOS
brew services start redis

# Or run directly
redis-server
```

### Run the Application

```bash
cd Project2.SignalRChat
dotnet run
```

The application will start at `http://localhost:5000` (or `https://localhost:5001`)

### Test Multiple Instances (Scaling)

Open multiple terminal windows and run:

```bash
# Terminal 1
dotnet run --urls "http://localhost:5000"

# Terminal 2
dotnet run --urls "http://localhost:5001"

# Terminal 3
dotnet run --urls "http://localhost:5002"
```

Open browsers to each URL - messages will sync across all instances via Redis!

**💡 Pro Tip**: Open Redis Commander at `http://localhost:8081` to watch messages flow through Redis in real-time!

## 🎮 How to Use

1. Open the application in your browser
2. Enter your username in the input field
3. Press Enter to join the chat
4. Type messages and click Send (or press Enter)
5. Open multiple browser tabs/windows to see real-time sync

## 🔧 Configuration

Edit `appsettings.json` to configure Redis connection:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "SignalRChat:"
  }
}
```

For production, use a connection string like:
```
"your-redis-server.com:6380,password=yourpassword,ssl=True"
```

## 📝 SignalR Hub Methods

### Client → Server

- `SendMessage(user, message)` - Broadcast message to all clients
- `JoinRoom(roomName)` - Join a specific chat room
- `SendMessageToRoom(roomName, user, message)` - Send message to specific room

### Server → Client

- `ReceiveMessage(user, message)` - Receive chat messages
- `SystemMessage(message)` - Receive system notifications

## 🔍 How Redis Backplane Works

When you scale to multiple server instances:

1. User A connects to Server 1
2. User B connects to Server 2
3. User A sends a message
4. Server 1 publishes to Redis
5. Redis broadcasts to all subscribed servers
6. Server 2 receives and sends to User B

This enables **horizontal scaling** without sticky sessions!

## 🌟 Key Benefits

- ✅ Scale horizontally across multiple servers
- ✅ No sticky sessions required
- ✅ Real-time bidirectional communication
- ✅ Automatic reconnection handling
- ✅ Group/room support for segmented chats
- ✅ Low latency with Redis pub/sub

## 📚 Learn More

- [ASP.NET Core SignalR](https://docs.microsoft.com/aspnet/core/signalr/)
- [SignalR with Redis](https://docs.microsoft.com/aspnet/core/signalr/redis-backplane)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)

## 🧪 Testing

To verify Redis is working:

```bash
# Connect to Redis CLI
redis-cli

# Monitor all commands (run in separate terminal)
redis-cli monitor

# You'll see SignalR publishing messages to Redis channels
```

## 🎨 UI Features

- Responsive gradient design
- Color-coded messages (user vs others)
- Connection status indicator
- Smooth animations
- Auto-scroll to latest message
- Timestamp on each message

## 🔐 CORS Configuration

The app is configured with CORS for development. For production, restrict to specific origins:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", builder =>
    {
        builder.WithOrigins("https://yourdomain.com")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
```

## 💡 Next Steps

- Add authentication with JWT
- Persist chat history in Redis or database
- Add private messaging
- Add file/image sharing
- Add typing indicators
- Add user presence (online/offline)
- Add message reactions
- Add chat room creation/management

---

**Happy Chatting! 🚀**
