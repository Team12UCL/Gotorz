using Shared.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Server.Services
{
    public class FlightService
    {
        private readonly HttpClient _httpClient;
        private readonly AmadeusAuthService _authService;
        public readonly string _baseUrl;

        public FlightService(IHttpClientFactory httpClientFactory, AmadeusAuthService authService, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("AmadeusClient");
            _authService = authService;
            _baseUrl = configuration["AmadeusAPI:FlightOffersUrl"]!;
        }

        public async Task<FlightOfferRootModel?> GetFlightOffersAsync(
            string originLocationCode,
            string destinationLocationCode,
            string departureDate,
            int adults)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(originLocationCode) ||
                    string.IsNullOrWhiteSpace(destinationLocationCode) ||
                    string.IsNullOrWhiteSpace(departureDate))
                {
                    Debug.WriteLine("Invalid search parameters.");
                    return null;
                }

                // Retrieve the bearer token using the auth service
                var token = await _authService.GetAccessTokenAsync();
                if (token == null)
                {
                    Debug.WriteLine("No token retrieved.");
                    return null;
                }

                // Set the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Build the full request URL
                string requestUrl = $"{_baseUrl}?originLocationCode={originLocationCode}&destinationLocationCode={destinationLocationCode}&departureDate={departureDate}&adults={adults}";

                // Make the API call
                var response = await _httpClient.GetAsync(requestUrl);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Error: {response.StatusCode}, {errorContent}");
                    return null;
                }

                // Read and deserialize the response
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Success: {content.Substring(0, Math.Min(200, content.Length))}...");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var flightOffers = JsonSerializer.Deserialize<FlightOfferRootModel>(content, options);

                // Log deserialization results
                if (flightOffers == null)
                {
                    Debug.WriteLine("Deserialization returned null.");
                }
                else
                {
                    Debug.WriteLine($"Successfully deserialized flight offers with {flightOffers.Data?.Count ?? 0} items.");
                }

                // For viewing info about the first JSON object in console
                //if (flightOffers.Data != null && flightOffers.Data.Count > 0)
                //{
                //    var firstFlight = flightOffers.Data[0];
                //    Debug.WriteLine($"First flight data: {JsonSerializer.Serialize(firstFlight)}");
                //}

                return flightOffers;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception occurred: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}

