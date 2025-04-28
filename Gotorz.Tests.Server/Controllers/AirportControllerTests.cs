using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Gotorz; // Your Server project

namespace Gotorz.Tests.Server.Controllers
{
    public class AirportControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AirportControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SuggestAirports_EmptyQuery_ReturnsBadRequest()
        {
            // Arrange
            var url = "api/airport/suggest-airports?query=";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SuggestAirports_ValidQuery_ReturnsOkWithResults()
        {
            // Arrange
            var url = "api/airport/suggest-airports?query=Lon"; // Assume "London" exists in airports.json

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var airports = await response.Content.ReadFromJsonAsync<List<Shared.Models.Airport>>();
            Assert.NotNull(airports);
            Assert.NotEmpty(airports); // Should find airports like London Heathrow, etc.
        }
    }
}