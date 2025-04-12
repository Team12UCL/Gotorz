using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.Models;

namespace Gotorz.Client.Services
{
    public class ChatService
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private HubConnection _hubConnection;
        private readonly List<ChatMessage> _messageHistory = new();
        private string _currentChatRoomId;

        public event Action<ChatMessage> OnMessageReceived;
        public event Action<string> OnUserJoined;
        public event Action<string> OnUserLeft;
        public event Action<string> OnTypingStarted;
        public event Action<string> OnTypingStopped;
        public event Action<ChatRoom> OnChatRoomCreated;
        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public ChatService(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
        }

        public async Task InitializeConnection(string userId)
        {
            if (_hubConnection != null)
            {
                return;
            }

            // Create the hub connection
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigationManager.ToAbsoluteUri("/chatHub"))
                .WithAutomaticReconnect()
                .Build();

            // Register handlers for hub methods
            _hubConnection.On<ChatMessage>("ReceiveMessage", (message) =>
            {
                _messageHistory.Add(message);
                OnMessageReceived?.Invoke(message);
            });

            _hubConnection.On<string>("UserJoined", (username) =>
            {
                OnUserJoined?.Invoke(username);
            });

            _hubConnection.On<string>("UserLeft", (username) =>
            {
                OnUserLeft?.Invoke(username);
            });

            _hubConnection.On<string>("UserIsTyping", (username) =>
            {
                OnTypingStarted?.Invoke(username);
            });

            _hubConnection.On<string>("UserStoppedTyping", (username) =>
            {
                OnTypingStopped?.Invoke(username);
            });

            _hubConnection.On<ChatRoom>("ChatRoomCreated", (chatRoom) =>
            {
                OnChatRoomCreated?.Invoke(chatRoom);
            });

            // Start the connection
            await _hubConnection.StartAsync();

            // Set the user identity
            await _hubConnection.SendAsync("SetUserIdentity", userId);
        }

        public async Task JoinChatRoom(string chatRoomId)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Chat connection is not established");
            }

            _currentChatRoomId = chatRoomId;
            await _hubConnection.SendAsync("JoinChatRoom", chatRoomId);

            // Load message history for this room
            await LoadMessageHistory(chatRoomId);
        }

        public async Task LeaveChatRoom(string chatRoomId)
        {
            if (!IsConnected)
            {
                return;
            }

            await _hubConnection.SendAsync("LeaveChatRoom", chatRoomId);
            _currentChatRoomId = null;
            _messageHistory.Clear();
        }

        public async Task SendMessage(string message, string userId, string username)
        {
            if (!IsConnected || string.IsNullOrEmpty(_currentChatRoomId))
            {
                throw new InvalidOperationException("Cannot send message: Not connected to chat room");
            }

            var chatMessage = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                ChatRoomId = _currentChatRoomId,
                SenderId = userId,
                SenderName = username,
                Content = message,
                Timestamp = DateTime.UtcNow
            };

            await _hubConnection.SendAsync("SendMessage", chatMessage);
        }

        public async Task NotifyTyping(bool isTyping)
        {
            if (!IsConnected || string.IsNullOrEmpty(_currentChatRoomId))
            {
                return;
            }

            if (isTyping)
            {
                await _hubConnection.SendAsync("NotifyTyping", _currentChatRoomId);
            }
            else
            {
                await _hubConnection.SendAsync("NotifyStoppedTyping", _currentChatRoomId);
            }
        }

        public async Task<IEnumerable<ChatRoom>> GetAvailableChatRooms()
        {
            try
            {
                var chatRooms = await _httpClient.GetFromJsonAsync<List<ChatRoom>>("api/chat/rooms");
                return chatRooms ?? new List<ChatRoom>();
            }
            catch (Exception)
            {
                // Log the error in a real app
                return new List<ChatRoom>();
            }
        }

        public async Task<ChatRoom> CreateSupportChatRoom(string userId, string username, string issue)
        {
            try
            {
                var chatRoom = new ChatRoom
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"Support: {issue}",
                    Type = "Support",
                    CreatedById = userId,
                    CreatedByName = username,
                    CreatedAt = DateTime.UtcNow,
                    IsClosed = false
                };

                var response = await _httpClient.PostAsJsonAsync("api/chat/rooms", chatRoom);
                if (response.IsSuccessStatusCode)
                {
                    var createdRoom = await response.Content.ReadFromJsonAsync<ChatRoom>();
                    return createdRoom;
                }

                return null;
            }
            catch (Exception)
            {
                // Log the error in a real app
                return null;
            }
        }

        public async Task<bool> CloseChatRoom(string chatRoomId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/chat/rooms/{chatRoomId}/close", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                // Log the error in a real app
                return false;
            }
        }

        private async Task LoadMessageHistory(string chatRoomId)
        {
            try
            {
                var messages = await _httpClient.GetFromJsonAsync<List<ChatMessage>>($"api/chat/rooms/{chatRoomId}/messages");
                if (messages != null)
                {
                    _messageHistory.Clear();
                    _messageHistory.AddRange(messages);
                }
            }
            catch (Exception)
            {
                // Log the error in a real app
            }
        }

        public IReadOnlyList<ChatMessage> GetMessageHistory()
        {
            return _messageHistory.AsReadOnly();
        }

        public async Task DisposeAsync()
        {
            if (_hubConnection != null)
            {
                if (!string.IsNullOrEmpty(_currentChatRoomId))
                {
                    await LeaveChatRoom(_currentChatRoomId);
                }

                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
    }
}