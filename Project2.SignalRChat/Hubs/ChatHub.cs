using Microsoft.AspNetCore.SignalR;

namespace Project2.SignalRChat.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("SystemMessage", $"User joined {roomName}");
    }

    public async Task SendMessageToRoom(string roomName, string user, string message)
    {
        await Clients.Group(roomName).SendAsync("ReceiveMessage", user, message);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("SystemMessage", $"User {Context.ConnectionId} connected");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("SystemMessage", $"User {Context.ConnectionId} disconnected");
        await base.OnDisconnectedAsync(exception);
    }
}
