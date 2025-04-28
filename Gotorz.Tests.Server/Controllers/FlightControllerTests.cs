using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Gotorz; // Your Server project

namespace Gotorz.Tests.Server.Controllers
{
    public class FlightControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public FlightControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Theory]
        [InlineData("", "JFK", "2025-05-01", 1)]  // Missing origin
        [InlineData("CDG", "", "2025-05-01", 1)]  // Missing destination
        [InlineData("CDG", "JFK", "", 1)]         // Missing departure date
        [InlineData("CDG", "JFK", "2025-05-01", 0)] // Invalid adults (0)
        public async Task GetFlightOffers_InvalidInputs_ReturnsBadRequest(string origin, string destination, string departureDate, int adults)
        {
            // Arrange
            var url = $"api/flight/search?originLocationCode={origin}&destinationLocationCode={destination}&departureDate={departureDate}&adults={adults}";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetFlightOffers_ValidParameters_ReturnsOk()
        {
            // Arrange
            var url = "api/flight/search?originLocationCode=CPH&destinationLocationCode=PAR&departureDate=2025-05-01&adults=1";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}