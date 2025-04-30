using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Gotorz;

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
            var url = "api/airport/suggest-airports?query=";

            var response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SuggestAirports_ValidQuery_ReturnsOkWithResults()
        {
            var url = "api/airport/suggest-airports?query=Lon";

            var response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var airports = await response.Content.ReadFromJsonAsync<List<Shared.Models.Airport>>();
            Assert.NotNull(airports);
            Assert.NotEmpty(airports);
        }
    }
}