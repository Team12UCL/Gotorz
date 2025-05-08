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
            await WaitUntilConnectedAsync(hubConnection);

            await Task.Delay(250);

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

            AttachHubConnectionLogging(hubConnectionUser1, "User1");
            AttachHubConnectionLogging(hubConnectionUser2, "User2");

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
            await WaitUntilConnectedAsync(hubConnectionUser1);

            await hubConnectionUser2.StartAsync();
            await WaitUntilConnectedAsync(hubConnectionUser2);

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

        private void AttachHubConnectionLogging(HubConnection connection, string name)
        {
            connection.Closed += async (error) =>
            {
                Console.WriteLine($"[{name}] Connection closed: {error?.Message}");
            };

            connection.Reconnecting += async (error) =>
            {
                Console.WriteLine($"[{name}] Reconnecting: {error?.Message}");
            };

            connection.Reconnected += async (connectionId) =>
            {
                Console.WriteLine($"[{name}] Reconnected with ID: {connectionId}");
            };
        }

        private async Task WaitUntilConnectedAsync(HubConnection connection, int timeoutMs = 5000)
        {
            var start = DateTime.UtcNow;
            while (connection.State != HubConnectionState.Connected && (DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
            {
                Console.WriteLine($"Waiting for connection... Current state: {connection.State}");
                await Task.Delay(100);
            }

            if (connection.State != HubConnectionState.Connected)
            {
                throw new InvalidOperationException("SignalR connection did not reach Connected state in time.");
            }
        }
    }
}