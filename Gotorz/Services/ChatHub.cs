using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Gotorz.Services
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinSupportRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserJoined", $"Support agent has joined the chat");
        }

        public async Task SendSupportMessage(string roomId, string user, string message)
        {
            await Clients.Group(roomId).SendAsync("ReceiveSupportMessage", user, message);
        }

        public async Task LeaveSupportRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserLeft", $"Support agent has left the chat");
        }
    }
}