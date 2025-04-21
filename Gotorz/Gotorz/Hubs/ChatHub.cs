using Gotorz.Data;
using Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class ChatHub(ApplicationDbContext context) : Hub
{
    private readonly ApplicationDbContext _context = context;

    public async Task SendMessage(string userId, string userName, string message)
    {
        var chatMessage = new ChatMessage
        {
            UserId = userId,
            UserName = userName,
            Text = message,
            Timestamp = DateTime.UtcNow
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        await Clients.All.SendAsync("ReceiveMessage", userId, userName, message);
    }

    public async Task Typing(string user)
    {
        await Clients.Others.SendAsync("UserTyping", user);
    }

    public async Task<List<ChatMessage>> GetMessageHistory()
    {
        return await _context.ChatMessages
            .OrderByDescending(m => m.Timestamp)
            .Take(50)
            .ToListAsync();
    }
}