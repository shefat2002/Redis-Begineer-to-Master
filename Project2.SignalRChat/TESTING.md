# Testing the Redis Backplane

This guide helps you verify that Redis is properly distributing SignalR messages across multiple server instances.

## Setup

### 1. Start Redis
```bash
# Option A: Docker
docker run -d -p 6379:6379 redis:latest

# Option B: Local installation
redis-server
```

### 2. Verify Redis is Running
```bash
redis-cli ping
# Should return: PONG
```

## Test 1: Single Instance (Basic Test)

```bash
cd Project2.SignalRChat
dotnet run
```

1. Open browser to `http://localhost:5000`
2. Enter username and send messages
3. Open another browser tab to same URL
4. Verify messages appear in both tabs

✅ **Result**: Messages sync within the same server instance

## Test 2: Multiple Instances (Scaling Test)

### Terminal 1: Start Server on Port 5000
```bash
cd Project2.SignalRChat
dotnet run --urls "http://localhost:5000"
```

### Terminal 2: Start Server on Port 5001
```bash
cd Project2.SignalRChat
dotnet run --urls "http://localhost:5001"
```

### Terminal 3: Start Server on Port 5002
```bash
cd Project2.SignalRChat
dotnet run --urls "http://localhost:5002"
```

### Terminal 4: Monitor Redis
```bash
redis-cli monitor
```

### Test Steps

1. **Browser A** → Open `http://localhost:5000`
   - Enter username: "Alice"
   
2. **Browser B** → Open `http://localhost:5001`
   - Enter username: "Bob"
   
3. **Browser C** → Open `http://localhost:5002`
   - Enter username: "Charlie"

4. **From Browser A** (Alice on port 5000):
   - Send message: "Hello from Server 1!"
   
5. **Verify**:
   - ✅ Bob sees message in Browser B (connected to 5001)
   - ✅ Charlie sees message in Browser C (connected to 5002)
   - ✅ Redis monitor shows `PUBLISH` commands

✅ **Result**: Messages distributed across ALL servers via Redis!

## Test 3: Monitor Redis Activity

### Watch Redis Channels
```bash
redis-cli
> PUBSUB CHANNELS
```

You should see channels like:
```
"Microsoft.AspNetCore.SignalR.Redis:*:internal:*"
"Microsoft.AspNetCore.SignalR.Redis:*:messages:all"
```

### Monitor Messages in Real-Time
```bash
redis-cli monitor
```

When you send a message, you'll see:
```
1702345678.123456 [0 127.0.0.1:12345] "PUBLISH" "...SignalRChat..." "..."
```

### Check Active Connections
```bash
redis-cli
> CLIENT LIST
```

You'll see connections from each server instance.

## Test 4: Test Reconnection

1. Start one server instance
2. Connect with browser
3. Stop the server (Ctrl+C)
4. Start the server again
5. Verify browser automatically reconnects

✅ **Result**: Automatic reconnection works

## Test 5: Test Groups (Rooms)

Open browser console (F12) and try:

```javascript
// Join a room
await connection.invoke("JoinRoom", "general");

// Send message to room
await connection.invoke("SendMessageToRoom", "general", "YourName", "Hello room!");
```

✅ **Result**: Only users in "general" room receive the message

## Expected Redis Behavior

### When a message is sent:

1. **Client** sends via WebSocket to Server 1
2. **Server 1** receives in SignalR Hub
3. **Server 1** calls `Clients.All.SendAsync()`
4. **SignalR** detects Redis backplane
5. **Redis** receives `PUBLISH` command
6. **Redis** broadcasts to ALL subscribed servers
7. **All Servers** receive from Redis
8. **All Servers** push to their connected clients
9. **All Clients** receive the message

### Redis Commands You'll See

```bash
# Server subscribes to channels on startup
SUBSCRIBE Microsoft.AspNetCore.SignalR.Redis:*:messages:all

# When message is sent
PUBLISH Microsoft.AspNetCore.SignalR.Redis:*:messages:all "{message data}"

# Periodic heartbeats
PUBLISH Microsoft.AspNetCore.SignalR.Redis:*:internal:* "{heartbeat}"
```

## Performance Test

### Load Test with Multiple Clients

```bash
# Install websocket test tool
npm install -g wscat

# Connect multiple clients
wscat -c ws://localhost:5000/chatHub
wscat -c ws://localhost:5001/chatHub
wscat -c ws://localhost:5002/chatHub
```

Or use the browser to open 10+ tabs across different ports.

### Check Redis Performance
```bash
redis-cli
> INFO stats
```

Look for:
- `total_commands_processed`: Should be increasing
- `pubsub_channels`: Number of active channels
- `pubsub_patterns`: Number of pattern subscriptions

## Troubleshooting

### Messages not syncing across instances?

**Check Redis connection:**
```bash
redis-cli ping
```

**Check server logs:**
Look for "Redis connection established" or errors.

**Verify Redis config in appsettings.json:**
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### Redis not starting?

```bash
# macOS
brew services restart redis

# Docker
docker ps  # Check if container is running
docker logs <container-id>
```

### Port already in use?

```bash
# Find what's using the port
lsof -i :5000

# Or use different ports
dotnet run --urls "http://localhost:6000"
```

## Verification Checklist

- [ ] Redis is running (`redis-cli ping` returns PONG)
- [ ] Single instance works (messages in same browser tabs)
- [ ] Multiple instances sync messages across servers
- [ ] Redis monitor shows PUBLISH commands
- [ ] Automatic reconnection works
- [ ] Multiple clients can connect simultaneously
- [ ] System messages show for connect/disconnect

## Success Indicators

✅ **Working Correctly If:**
- User on Server 1 can chat with user on Server 2
- Redis monitor shows message activity
- All clients receive messages regardless of server
- No errors in server logs
- Reconnection happens automatically

❌ **Not Working If:**
- Messages only appear in same browser/server
- Redis monitor is silent
- Console shows connection errors
- Messages are lost

## Advanced: Redis Cluster Testing

For production-scale testing with Redis Cluster:

```json
{
  "Redis": {
    "ConnectionString": "server1:6379,server2:6379,server3:6379",
    "InstanceName": "SignalRChat:"
  }
}
```

## Load Testing Script

Create `load-test.js`:
```javascript
const signalR = require("@microsoft/signalr");

async function createClient(port, username) {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`http://localhost:${port}/chatHub`)
        .build();
    
    await connection.start();
    console.log(`${username} connected to port ${port}`);
    
    connection.on("ReceiveMessage", (user, message) => {
        console.log(`[${username}] Received from ${user}: ${message}`);
    });
    
    return connection;
}

async function test() {
    const clients = [];
    clients.push(await createClient(5000, "User1"));
    clients.push(await createClient(5001, "User2"));
    clients.push(await createClient(5002, "User3"));
    
    // Send messages from different servers
    await clients[0].invoke("SendMessage", "User1", "From Server 1");
    await clients[1].invoke("SendMessage", "User2", "From Server 2");
    await clients[2].invoke("SendMessage", "User3", "From Server 3");
}

test();
```

---

## Summary

This testing guide proves that Redis successfully distributes SignalR messages across multiple server instances, enabling true horizontal scaling without sticky sessions. The Redis backplane pattern is production-ready and used by major applications worldwide.

**Key Takeaway**: Any server can receive a message, Redis distributes it, and ALL servers forward it to their clients. This is the power of the backplane pattern! 🚀
