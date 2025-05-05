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

            hubConnection.On<string, string, string>("ReceiveMessage", (userId, userName, message) =>
            {
                receivedMessages.Add((userId, userName, message));
            });

            await hubConnection.StartAsync();

            string testUserId = "test-user-id";
            string testUserName = "TestUser";
            string testMessage = "Hello from Integration Test!";

            await hubConnection.SendAsync("SendMessage", testUserId, testUserName, testMessage);

            await Task.Delay(500);

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

            List<(string UserId, string UserName, string Text)> receivedByUser1 = new();
            List<(string UserId, string UserName, string Text)> receivedByUser2 = new();

            hubConnectionUser1.On<string, string, string>("ReceiveMessage", (userId, userName, message) =>
            {
                receivedByUser1.Add((userId, userName, message));
            });

            hubConnectionUser2.On<string, string, string>("ReceiveMessage", (userId, userName, message) =>
            {
                receivedByUser2.Add((userId, userName, message));
            });

            await hubConnectionUser1.StartAsync();
            await hubConnectionUser2.StartAsync();

            string user1Id = "user1-id";
            string user1Name = "User1";
            string messageFromUser1 = "Hello from User1!";

            await hubConnectionUser1.SendAsync("SendMessage", user1Id, user1Name, messageFromUser1);

            await Task.Delay(500);

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

            await hubConnectionUser1.DisposeAsync();
            await hubConnectionUser2.DisposeAsync();
        }

        [Fact]
        public async Task ChatHub_UserTyping_BroadcastsTypingNotification()
        {
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

            hubConnectionUser2.On<string>("UserTyping", (userName) =>
            {
                typingNotificationReceived = userName;
            });

            await hubConnectionUser1.StartAsync();
            await hubConnectionUser2.StartAsync();

            string typingUserName = "TypingUser";

            await hubConnectionUser1.SendAsync("Typing", typingUserName);

            await Task.Delay(500);

            Assert.Equal(typingUserName, typingNotificationReceived);

            await hubConnectionUser1.DisposeAsync();
            await hubConnectionUser2.DisposeAsync();
        }

        [Fact]
        public async Task ChatHub_ManyClients_CanBroadcastMessages()
        {
            var client = _factory.CreateDefaultClient();
            var baseUri = client.BaseAddress?.ToString() ?? "https://localhost:5001";

            const int clientCount = 50;
            var connections = new List<HubConnection>();
            var receivedMessages = new List<(string UserId, string UserName, string Text)>();

            for (int i = 0; i < clientCount; i++)
            {
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl($"{baseUri}chathub", options =>
                    {
                        options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                    })
                    .WithAutomaticReconnect()
                    .Build();

                hubConnection.On<string, string, string>("ReceiveMessage", (userId, userName, text) =>
                {
                    lock (receivedMessages)
                    {
                        receivedMessages.Add((userId, userName, text));
                    }
                });

                connections.Add(hubConnection);
            }

            await Task.WhenAll(connections.Select(c => c.StartAsync()));

            var random = new Random();
            var senders = connections.OrderBy(_ => random.Next()).Take(5).ToList();

            foreach (var sender in senders)
            {
                string userId = Guid.NewGuid().ToString();
                string userName = "StressUser";
                string message = "Stress Test Message";

                await sender.SendAsync("SendMessage", userId, userName, message);
            }

            await Task.Delay(3000);

            int expectedMinimumMessages = senders.Count * connections.Count;

            lock (receivedMessages)
            {
                Assert.True(receivedMessages.Count >= expectedMinimumMessages, $"Expected at least {expectedMinimumMessages} messages but got {receivedMessages.Count}");
            }

            await Task.WhenAll(connections.Select(c => c.DisposeAsync().AsTask()));
        }

        [Fact]
        public async Task ChatHub_GetMessageHistory_ReturnsRecentMessages()
        {
            var client = _factory.CreateDefaultClient();
            var baseUri = client.BaseAddress?.ToString() ?? "https://localhost:5001";

            await ClearChatMessages();

            var testTag = "[TestRun]";
            var messagesToSend = new List<(string UserId, string UserName, string Text)>
    {
        (Guid.NewGuid().ToString(), "User1", $"{testTag} First message!"),
        (Guid.NewGuid().ToString(), "User2", $"{testTag} Second message!"),
        (Guid.NewGuid().ToString(), "User3", $"{testTag} Third message!")
    };

            var senderConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUri}chathub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                })
                .WithAutomaticReconnect()
                .Build();

            await senderConnection.StartAsync();

            foreach (var msg in messagesToSend)
            {
                await senderConnection.SendAsync("SendMessage", msg.UserId, msg.UserName, msg.Text);
            }

            await senderConnection.DisposeAsync();

            await Task.Delay(500);

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

            Assert.NotNull(history);

            var testMessagesInHistory = history.Where(m => m.Text.Contains(testTag)).ToList();

            foreach (var sent in messagesToSend)
            {
                Assert.Contains(testMessagesInHistory, m =>
                    m.Text.Trim().Equals(sent.Text.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    m.UserName.Trim().Equals(sent.UserName.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            Assert.Equal(messagesToSend.Count, testMessagesInHistory.Count);

            await ClearChatMessages();
        }
        private async Task ClearChatMessages()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.ChatMessages.RemoveRange(db.ChatMessages);
            await db.SaveChangesAsync();
        }
    }

}