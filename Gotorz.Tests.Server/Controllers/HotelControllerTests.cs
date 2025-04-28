using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Gotorz; // Your Server project

namespace Gotorz.Tests.Server.Controllers
{
    public class HotelControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public HotelControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Theory]
        [InlineData("")] // Empty city name
        public async Task SuggestCities_InvalidQuery_ReturnsBadRequest(string query)
        {
            // Arrange
            var url = $"api/hotel/suggest-cities?query={query}";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetHotelOffers_MissingParameters_ReturnsBadRequest()
        {
            // Missing cityCode, checkInDate, checkOutDate
            var url = "api/hotel/search?cityCode=&checkInDate=&checkOutDate=&adults=1";

            var response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetCityCode_EmptyCityName_ReturnsBadRequest()
        {
            // Arrange
            var url = "api/hotel/get-city-code?cityName=";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SuggestCities_ValidQuery_ReturnsOkWithResults()
        {
            // Arrange
            var url = "api/hotel/suggest-cities?query=Paris"; // Assume Amadeus API or local test data responds

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var cities = await response.Content.ReadFromJsonAsync<List<Shared.Models.AmadeusCityResponse.CityData>>();
            Assert.NotNull(cities);
            Assert.NotEmpty(cities);
        }

        [Fact]
        public async Task GetCityCode_ValidCityName_ReturnsCityCode()
        {
            // Arrange
            var url = "api/hotel/get-city-code?cityName=Paris";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.NotNull(payload);
            Assert.True(payload.ContainsKey("cityCode"));
        }
    }
}