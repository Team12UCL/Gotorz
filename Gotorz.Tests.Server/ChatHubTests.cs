using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.Models;
using System.Collections.Generic;
using System.Linq;
using Gotorz;
using Gotorz.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Gotorz.Tests.Server
{
    public class ChatHubTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ChatHubTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ChatHub_SendMessage_BroadcastsToClients()
        {
            // Arrange
            var client = _factory.CreateDefaultClient();
            var baseUri = client.BaseAddress?.ToString() ?? "https://localhost:5001";

            var hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUri}chathub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                })
                .WithAutomaticReconnect()
                .Build();

            List<(string UserId, string UserName, string Text)> receivedMessages = new();

            // Hook into receiving messages BEFORE starting
            hubConnection.On<string, string, string>("ReceiveMessage", (userId, userName, message) =>
            {
                receivedMessages.Add((userId, userName, message));
            });

            await hubConnection.StartAsync(); // ✅ Start only once after handlers

            // Act
            string testUserId = "test-user-id";
            string testUserName = "TestUser";
            string testMessage = "Hello from Integration Test!";

            await hubConnection.SendAsync("SendMessage", testUserId, testUserName, testMessage);

            await Task.Delay(500);

            // Assert
            Assert.Contains(receivedMessages, msg =>
                msg.UserId == testUserId &&
                msg.UserName == testUserName &&
                msg.Text == testMessage
            );

            await hubConnection.DisposeAsync();
        }

        [Fact]
        public async Task ChatHub_MultipleClients_ReceiveEachOthersMessages()
        {
            // Arrange: Start the backend server
            var client = _factory.CreateDefaultClient();
            var baseUri = client.BaseAddress?.ToString() ?? "https://localhost:5001";

            // Create two different HubConnections
            var hubConnectionUser1 = new HubConnectionBuilder()
                .WithUrl($"{baseUri}chathub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                })
                .WithAutomaticReconnect()
                .Build();

            var hubConnectionUser2 = new HubConnectionBuilder()
                .WithUrl($"{baseUri}chathub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                })
                .WithAutomaticReconnect()
                .Build();

            // Lists to store received messages
            List<(string UserId, string UserName, string Text)> receivedByUser1 = new();
            List<(string UserId, string UserName, string Text)> receivedByUser2 = new();

            // Set up handlers
            hubConnectionUser1.On<string, string, string>("ReceiveMessage", (userId, userName, message) =>
            {
                receivedByUser1.Add((userId, userName, message));
            });

            hubConnectionUser2.On<string, string, string>("ReceiveMessage", (userId, userName, message) =>
            {
                receivedByUser2.Add((userId, userName, message));
            });

            // Start both connections
            await hubConnectionUser1.StartAsync();
            await hubConnectionUser2.StartAsync();

            // Act: User1 sends a message
            string user1Id = "user1-id";
            string user1Name = "User1";
            string messageFromUser1 = "Hello from User1!";

            await hubConnectionUser1.SendAsync("SendMessage", user1Id, user1Name, messageFromUser1);

            // Give time for messages to be processed
            await Task.Delay(500);

            // Assert: Both users should have received the message
            Assert.Contains(receivedByUser1, msg =>
                msg.UserId == user1Id &&
                msg.UserName == user1Name &&
                msg.Text == messageFromUser1
            );

            Assert.Contains(receivedByUser2, msg =>
                msg.UserId == user1Id &&
                msg.UserName == user1Name &&
                msg.Text == messageFromUser1
            );

            // Cleanup
            await hubConnectionUser1.DisposeAsync();
            await hubConnectionUser2.DisposeAsync();
        }

        [Fact]
        public async Task ChatHub_UserTyping_BroadcastsTypingNotification()
        {
            // Arrange: Start real backend server
            var client = _factory.CreateDefaultClient();
            var baseUri = client.BaseAddress?.ToString() ?? "https://localhost:5001";

            var hubConnectionUser1 = new HubConnectionBuilder()
                .WithUrl($"{baseUri}chathub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                })
                .WithAutomaticReconnect()
                .Build();

            var hubConnectionUser2 = new HubConnectionBuilder()
                .WithUrl($"{baseUri}chathub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                })
                .WithAutomaticReconnect()
                .Build();

            string? typingNotificationReceived = null;

            // Subscribe to typing notification BEFORE starting
            hubConnectionUser2.On<string>("UserTyping", (userName) =>
            {
                typingNotificationReceived = userName;
            });

            await hubConnectionUser1.StartAsync();
            await hubConnectionUser2.StartAsync();

            // Act: user1 sends typing event
            string typingUserName = "TypingUser";

            await hubConnectionUser1.SendAsync("Typing", typingUserName);

            // Wait for notification to arrive
            await Task.Delay(500);

            // Assert: user2 received the typing notification
            Assert.Equal(typingUserName, typingNotificationReceived);

            // Cleanup
            await hubConnectionUser1.DisposeAsync();
            await hubConnectionUser2.DisposeAsync();
        }

        [Fact]
        public async Task ChatHub_ManyClients_CanBroadcastMessages()
        {
            // Arrange
            var client = _factory.CreateDefaultClient();
            var baseUri = client.BaseAddress?.ToString() ?? "https://localhost:5001";

            const int clientCount = 50; // 🔥 Adjust this number: 10, 20, 50 for more stress
            var connections = new List<HubConnection>();
            var receivedMessages = new List<(string UserId, string UserName, string Text)>();

            // Create multiple hub connections
            for (int i = 0; i < clientCount; i++)
            {
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl($"{baseUri}chathub", options =>
                    {
                        options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                    })
                    .WithAutomaticReconnect()
                    .Build();

                // Every client listens for messages
                hubConnection.On<string, string, string>("ReceiveMessage", (userId, userName, text) =>
                {
                    lock (receivedMessages) // ⚡ Lock because multiple threads could update
                    {
                        receivedMessages.Add((userId, userName, text));
                    }
                });

                connections.Add(hubConnection);
            }

            // Start all connections
            await Task.WhenAll(connections.Select(c => c.StartAsync()));
            // Pick 5 random clients to send messages
            var random = new Random();
            var senders = connections.OrderBy(_ => random.Next()).Take(5).ToList();

            foreach (var sender in senders)
            {
                string userId = Guid.NewGuid().ToString();
                string userName = "StressUser";
                string message = "Stress Test Message";

                await sender.SendAsync("SendMessage", userId, userName, message);
            }

            // Give some time for all messages to propagate
            await Task.Delay(3000);

            // Assert
            // Expect at least (senders.Count * connections.Count) messages total
            int expectedMinimumMessages = senders.Count * connections.Count;

            lock (receivedMessages)
            {
                Assert.True(receivedMessages.Count >= expectedMinimumMessages, $"Expected at least {expectedMinimumMessages} messages but got {receivedMessages.Count}");
            }

            // Cleanup
            await Task.WhenAll(connections.Select(c => c.DisposeAsync().AsTask()));
        }

        [Fact]
        public async Task ChatHub_GetMessageHistory_ReturnsRecentMessages()
        {
            // Arrange
            var client = _factory.CreateDefaultClient();
            var baseUri = client.BaseAddress?.ToString() ?? "https://localhost:5001";

            // 🔥 Clean up old chat messages
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.ChatMessages.RemoveRange(db.ChatMessages);
                await db.SaveChangesAsync();
            }

            // Create one client connection
            var senderConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUri}chathub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                })
                .WithAutomaticReconnect()
                .Build();

            await senderConnection.StartAsync();

            // Send a few messages
            var messagesToSend = new List<(string UserId, string UserName, string Text)>
    {
        (Guid.NewGuid().ToString(), "User1", "First message!"),
        (Guid.NewGuid().ToString(), "User2", "Second message!"),
        (Guid.NewGuid().ToString(), "User3", "Third message!")
    };

            foreach (var msg in messagesToSend)
            {
                await senderConnection.SendAsync("SendMessage", msg.UserId, msg.UserName, msg.Text);
            }

            await senderConnection.DisposeAsync();

            // Act
            var historyConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUri}chathub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                })
                .WithAutomaticReconnect()
                .Build();

            await historyConnection.StartAsync();

            var history = await historyConnection.InvokeAsync<List<ChatMessage>>("GetMessageHistory");

            await historyConnection.DisposeAsync();

            // Assert
            Assert.NotNull(history);
            Assert.NotEmpty(history);

            foreach (var sent in messagesToSend)
            {
                Assert.Contains(history, m => m.Text == sent.Text && m.UserName == sent.UserName);
            }
        }
    }
}