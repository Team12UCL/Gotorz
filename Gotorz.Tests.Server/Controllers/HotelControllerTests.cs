using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Gotorz;

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
        [InlineData("")]
        public async Task SuggestCities_InvalidQuery_ReturnsBadRequest(string query)
        {
            var url = $"api/hotel/suggest-cities?query={query}";

            var response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetHotelOffers_MissingParameters_ReturnsBadRequest()
        {
            var url = "api/hotel/search?cityCode=&checkInDate=&checkOutDate=&adults=1";

            var response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetCityCode_EmptyCityName_ReturnsBadRequest()
        {
            var url = "api/hotel/get-city-code?cityName=";

            var response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SuggestCities_ValidQuery_ReturnsOkWithResults()
        {
            var url = "api/hotel/suggest-cities?query=Paris";

            var response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var cities = await response.Content.ReadFromJsonAsync<List<Shared.Models.AmadeusCityResponse.CityData>>();
            Assert.NotNull(cities);
            Assert.NotEmpty(cities);
        }

        [Fact]
        public async Task GetCityCode_ValidCityName_ReturnsCityCode()
        {
            var url = "api/hotel/get-city-code?cityName=Paris";

            var response = await _client.GetAsync(url);

            Assert.True(response.IsSuccessStatusCode);

            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.NotNull(payload);
            Assert.True(payload.ContainsKey("cityCode"));
        }
    }
}