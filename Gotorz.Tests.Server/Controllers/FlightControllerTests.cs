using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Gotorz;

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
        [InlineData("", "JFK", "2025-07-01", 1)]
        [InlineData("CDG", "", "2025-07-01", 1)]
        [InlineData("CDG", "JFK", "", 1)]
        [InlineData("CDG", "JFK", "2025-07-01", 0)]
        public async Task GetFlightOffers_InvalidInputs_ReturnsBadRequest(string origin, string destination, string departureDate, int adults)
        {
            var url = $"api/flight/search?originLocationCode={origin}&destinationLocationCode={destination}&departureDate={departureDate}&adults={adults}";

            var response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetFlightOffers_ValidParameters_ReturnsOk()
        {
            var url = "api/flight/search?originLocationCode=CPH&destinationLocationCode=PAR&departureDate=2025-07-01&adults=1";

            var response = await _client.GetAsync(url);

            Assert.True(response.IsSuccessStatusCode);
        }
    }
}