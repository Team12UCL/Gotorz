using System;
using System.Collections.Generic;

namespace Shared.Models
{
    public enum ChatRoomType
    {
        Public,
        Private,
        Destination,
        Support
    }

    public class ChatRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ChatRoomType Type { get; set; }
        public List<string> Participants { get; set; } = new List<string>();
    }
}