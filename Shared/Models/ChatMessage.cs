using System;
using System.Collections.Generic;

namespace Shared.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ChatRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ChatRoomType Type { get; set; }
        public List<string> Participants { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum ChatRoomType
    {
        Support,
        Destination,
        Private
    }
}