using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gotorz.Client.Services
{
    public class ChatService
    {
        private static readonly List<ChatMessage> _messages = new List<ChatMessage>();
        private static readonly List<ChatRoom> _chatRooms = new List<ChatRoom>();
        private static int _nextMessageId = 1;
        private static int _nextRoomId = 1;

        // Event to notify subscribers when a new message is received
        public event Action<ChatMessage> OnMessageReceived;

        public ChatService()
        {
            // Add some predefined chat rooms if none exist
            if (_chatRooms.Count == 0)
            {
                // Create general room
                AddChatRoom(new ChatRoom
                {
                    Id = _nextRoomId++,
                    Name = "General",
                    Description = "General discussion for all travelers",
                    Type = ChatRoomType.Public
                });

                // Create destination-specific rooms
                AddChatRoom(new ChatRoom
                {
                    Id = _nextRoomId++,
                    Name = "Paris Travelers",
                    Description = "For travelers to Paris",
                    Type = ChatRoomType.Destination
                });

                AddChatRoom(new ChatRoom
                {
                    Id = _nextRoomId++,
                    Name = "Rome Travelers",
                    Description = "For travelers to Rome",
                    Type = ChatRoomType.Destination
                });

                // Create support room
                AddChatRoom(new ChatRoom
                {
                    Id = _nextRoomId++,
                    Name = "Customer Support",
                    Description = "Get help from our travel agents",
                    Type = ChatRoomType.Support
                });
            }

            // Add some mock messages if none exist
            if (_messages.Count == 0)
            {
                // Add messages to General room
                AddChatMessage(new ChatMessage
                {
                    Id = _nextMessageId++,
                    RoomId = 1,
                    SenderId = "system",
                    SenderName = "System",
                    Content = "Welcome to the General chat!",
                    Timestamp = DateTime.Now.AddHours(-5)
                });

                AddChatMessage(new ChatMessage
                {
                    Id = _nextMessageId++,
                    RoomId = 1,
                    SenderId = "user1",
                    SenderName = "Alex",
                    Content = "Hello everyone! I'm planning a trip to Paris next month.",
                    Timestamp = DateTime.Now.AddHours(-4)
                });

                AddChatMessage(new ChatMessage
                {
                    Id = _nextMessageId++,
                    RoomId = 1,
                    SenderId = "user2",
                    SenderName = "Maya",
                    Content = "That sounds exciting! I was there last summer, it was beautiful.",
                    Timestamp = DateTime.Now.AddHours(-4)
                });

                // Add messages to Customer Support room
                AddChatMessage(new ChatMessage
                {
                    Id = _nextMessageId++,
                    RoomId = 4,
                    SenderId = "system",
                    SenderName = "System",
                    Content = "Welcome to Customer Support. An agent will be with you shortly.",
                    Timestamp = DateTime.Now.AddHours(-2)
                });

                AddChatMessage(new ChatMessage
                {
                    Id = _nextMessageId++,
                    RoomId = 4,
                    SenderId = "agent1",
                    SenderName = "Sarah (Agent)",
                    Content = "Hello! How can I help you today?",
                    Timestamp = DateTime.Now.AddHours(-1)
                });
            }
        }

        // Get all chat rooms
        public List<ChatRoom> GetChatRooms()
        {
            return _chatRooms.ToList();
        }

        // Get chat room by ID
        public ChatRoom GetChatRoomById(int roomId)
        {
            return _chatRooms.FirstOrDefault(r => r.Id == roomId);
        }

        // Get messages for a specific room
        public List<ChatMessage> GetMessagesForRoom(int roomId)
        {
            return _messages.Where(m => m.RoomId == roomId)
                .OrderBy(m => m.Timestamp)
                .ToList();
        }

        // Add a new chat room
        public void AddChatRoom(ChatRoom room)
        {
            if (room.Id == 0)
            {
                room.Id = _nextRoomId++;
            }

            _chatRooms.Add(room);
        }

        // Add a new chat message
        public void AddChatMessage(ChatMessage message)
        {
            if (message.Id == 0)
            {
                message.Id = _nextMessageId++;
            }

            if (message.Timestamp == default)
            {
                message.Timestamp = DateTime.Now;
            }

            _messages.Add(message);

            // Notify subscribers
            OnMessageReceived?.Invoke(message);
        }

        // Create a new private chat between two users
        public ChatRoom CreatePrivateChat(string user1Id, string user2Id)
        {
            // Check if a private chat already exists
            var existingRoom = _chatRooms.FirstOrDefault(r =>
                r.Type == ChatRoomType.Private &&
                r.Participants.Contains(user1Id) &&
                r.Participants.Contains(user2Id));

            if (existingRoom != null)
            {
                return existingRoom;
            }

            // Create new private chat room
            var room = new ChatRoom
            {
                Id = _nextRoomId++,
                Name = $"Private Chat",
                Type = ChatRoomType.Private,
                Participants = new List<string> { user1Id, user2Id }
            };

            _chatRooms.Add(room);

            // Add a system message
            AddChatMessage(new ChatMessage
            {
                RoomId = room.Id,
                SenderId = "system",
                SenderName = "System",
                Content = "New private chat started.",
                Timestamp = DateTime.Now
            });

            return room;
        }

        // Simulate agent response (for mock purposes)
        public async Task<ChatMessage> SimulateAgentResponse(int roomId, string userMessage)
        {
            // Simulate typing delay
            await Task.Delay(2000);

            var room = GetChatRoomById(roomId);
            if (room == null || room.Type != ChatRoomType.Support)
            {
                return null;
            }

            // Create agent response based on user message
            string responseContent;

            if (userMessage.Contains("book", StringComparison.OrdinalIgnoreCase) ||
                userMessage.Contains("reservation", StringComparison.OrdinalIgnoreCase))
            {
                responseContent = "I'd be happy to help you with your booking. Could you please provide your booking reference number?";
            }
            else if (userMessage.Contains("cancel", StringComparison.OrdinalIgnoreCase))
            {
                responseContent = "To cancel a booking, we'll need your booking reference number. Please note that cancellation fees may apply depending on how close you are to your travel date.";
            }
            else if (userMessage.Contains("refund", StringComparison.OrdinalIgnoreCase))
            {
                responseContent = "Refunds are typically processed within 5-7 business days after a cancellation is confirmed. Would you like me to check the status of your refund?";
            }
            else if (userMessage.Contains("flight", StringComparison.OrdinalIgnoreCase) &&
                     userMessage.Contains("delay", StringComparison.OrdinalIgnoreCase))
            {
                responseContent = "I'm sorry to hear about your flight delay. Please provide your booking details, and I'll check the latest status for you.";
            }
            else
            {
                responseContent = "Thank you for your message. How else can I assist you with your travel plans today?";
            }

            // Create and add the agent message
            var message = new ChatMessage
            {
                RoomId = roomId,
                SenderId = "agent1",
                SenderName = "Sarah (Agent)",
                Content = responseContent,
                Timestamp = DateTime.Now
            };

            AddChatMessage(message);

            return message;
        }
    }
}