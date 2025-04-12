using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Gotorz.Services
{
    public class HotelService
    {
        private readonly HttpClient _httpClient;
        private readonly AmadeusAuthService _authService;
        private readonly ILogger<HotelService> _logger;

        public HotelService(HttpClient httpClient, AmadeusAuthService authService, ILogger<HotelService> logger)
        {
            _httpClient = httpClient;
            _authService = authService;
            _logger = logger;
        }

        public async Task<HotelOfferRootModel> SearchHotels(string cityCode, DateTime checkInDate,
            DateTime checkOutDate, int adults = 1, int radius = 50, string radiusUnit = "KM")
        {
            try
            {
                var token = await _authService.GetAccessToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var formattedCheckIn = checkInDate.ToString("yyyy-MM-dd");
                var formattedCheckOut = checkOutDate.ToString("yyyy-MM-dd");

                var url = $"https://test.api.amadeus.com/v2/shopping/hotel-offers?" +
                          $"cityCode={cityCode}&" +
                          $"checkInDate={formattedCheckIn}&" +
                          $"checkOutDate={formattedCheckOut}&" +
                          $"adults={adults}&" +
                          $"radius={radius}&" +
                          $"radiusUnit={radiusUnit}&" +
                          $"currency=USD&" +
                          $"includeClosed=false&" +
                          $"bestRateOnly=true&" +
                          $"view=FULL&" +
                          $"sort=PRICE";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var hotelRoot = JsonSerializer.Deserialize<HotelOfferRootModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return hotelRoot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching hotels");
                throw;
            }
        }
    }
}