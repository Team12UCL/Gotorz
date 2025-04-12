using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Shared.Models;

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
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(cityCode) ||
                    string.IsNullOrWhiteSpace(checkInDate) ||
                    string.IsNullOrWhiteSpace(checkOutDate))
                {
                    Debug.WriteLine("Invalid hotel search parameters.");
                    return null;
                }

                var token = await _authService.GetAccessTokenAsync();
                if (token == null)
                {
                    Debug.WriteLine("No token retrieved for hotel search.");
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Get hotel IDs for the city
                var hotelIds = await GetHotelIdsByCityAsync(cityCode);
                Debug.WriteLine($"🏨 Found {hotelIds.Count} hotel IDs for {cityCode}");

                if (hotelIds == null || hotelIds.Count == 0)
                {
                    Debug.WriteLine($"No hotels found for city code: {cityCode}");
                    return new HotelOfferRootModel { Data = new List<HotelOffer>() };
                }

                // Limit to 20 hotels to prevent excessive API calls
                var hotelIdsParam = string.Join(",", hotelIds.Take(20));

                // Build full URL with parameters
                var url = $"{_hotelOffersBaseUrl}?hotelIds={hotelIdsParam}&adults={adults}&checkInDate={checkInDate}&checkOutDate={checkOutDate}&currency=EUR&roomQuantity=1";
                Debug.WriteLine($"Requesting hotel offers: {url}");

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Error fetching hotel offers: {response.StatusCode}, {content}");
                    return new HotelOfferRootModel { Data = new List<HotelOffer>() };
                }

                Debug.WriteLine($"Hotel offers response: {content.Substring(0, Math.Min(200, content.Length))}...");

                var result = JsonSerializer.Deserialize<HotelOfferRootModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? new HotelOfferRootModel { Data = new List<HotelOffer>() };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in hotel search: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new HotelOfferRootModel { Data = new List<HotelOffer>() };
            }
        }

        private async Task<List<string>> GetHotelIdsByCityAsync(string cityCode)
        {
            try
            {
                // Construct URL with the city code
                var url = $"{_hotelsByCityBaseUrl}?cityCode={cityCode}&radius=20&radiusUnit=KM&hotelSource=ALL";
                Debug.WriteLine($"Requesting hotels by city: {url}");

                // Make the API call
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Error fetching hotels by city: {response.StatusCode}, {content}");
                    return new List<string>();
                }

                // Parse the response to extract hotel IDs
                var json = JsonDocument.Parse(content);
                var hotelIds = json.RootElement
                    .GetProperty("data")
                    .EnumerateArray()
                    .Select(h => h.GetProperty("hotelId").GetString())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToList();

                // Convert possible null strings to non-null strings
                return hotelIds.Select(id => id ?? string.Empty).Where(id => !string.IsNullOrEmpty(id)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting hotel IDs: {ex.Message}");
                return new List<string>();
            }
        }
    }
}
