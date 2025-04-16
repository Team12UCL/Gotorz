using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Shared.Models;
using Shared.Models.AmadeusCityResponse;

namespace Server.Services
{
    public class HotelService
    {
        private readonly HttpClient _httpClient;
        private readonly AmadeusAuthService _authService;
        private readonly string _hotelOffersBaseUrl;
        private readonly string _hotelsByCityBaseUrl;

        public HotelService(IHttpClientFactory httpClientFactory, AmadeusAuthService authService, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient("AmadeusClient");
            _authService = authService;
            _hotelOffersBaseUrl = config["AmadeusAPI:HotelOffersUrl"]!;
            _hotelsByCityBaseUrl = config["AmadeusAPI:HotelsByCityUrl"]!;
        }

        public async Task<HotelOfferRootModel?> GetHotelOffersAsync(string cityCode, string checkInDate, string checkOutDate, int adults)
        {
            try
            {

                var token = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var hotelIds = await GetHotelIdsByCityAsync(cityCode);
                Console.WriteLine($"🏨 Found {hotelIds.Count} hotel IDs for {cityCode}");

                if (hotelIds == null || hotelIds.Count == 0)
                {
                    return null;
                }

                var hotelIdsParam = string.Join(",", hotelIds.Take(20));

                var url = $"{_hotelOffersBaseUrl}?hotelIds={hotelIdsParam}&&adults={adults}&checkInDate={checkInDate}&checkOutDate={checkOutDate}";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var result = JsonSerializer.Deserialize<HotelOfferRootModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return null;
            }
        }
        public async Task<List<CityData>> GetCitySuggestionsAsync(string query)
        {
            var token = await _authService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"https://test.api.amadeus.com/v1/reference-data/locations/cities?keyword={query}&max=10";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<CityData>();

            var result = await response.Content.ReadFromJsonAsync<AmadeusCityResponse>();
            return result?.Data ?? new List<CityData>();
        }

        private async Task<List<string>> GetHotelIdsByCityAsync(string cityCode)
        {
            try
            {
                var url = $"{_hotelsByCityBaseUrl}?cityCode={cityCode}";
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new List<string>();
                }

                var json = JsonDocument.Parse(content);
                var hotelIds = json.RootElement
                    .GetProperty("data")
                    .EnumerateArray()
                    .Select(h => h.GetProperty("hotelId").GetString())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToList();

                return hotelIds;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }
        public async Task<string?> GetCityCodeAsync(string cityName)
        {
            var token = await _authService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"https://test.api.amadeus.com/v1/reference-data/locations?subType=CITY&keyword={cityName}&page[limit]=1";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("data")
                .EnumerateArray()
                .FirstOrDefault()
                .GetProperty("iataCode")
                .GetString();
        }
    }
}
