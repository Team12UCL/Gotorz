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
        private readonly string _baseUrl;
        private readonly ILogger<FlightService> _logger;

        public FlightService(IHttpClientFactory httpClientFactory, AmadeusAuthService authService, IConfiguration configuration, ILogger<FlightService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("AmadeusClient");
            _authService = authService;
            _baseUrl = configuration["AmadeusAPI:FlightOffersUrl"]!;
            _logger = logger;
        }

        public async Task<FlightOfferRootModel?> GetFlightOffersAsync(
            string originLocationCode,
            string destinationLocationCode,
            string departureDate,
            int adults = 1,
            string? returnDate = null,
            string travelClass = "ECONOMY",
            bool nonStop = false,
            string currencyCode = "EUR",
            int max = 10)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(originLocationCode) ||
                    string.IsNullOrWhiteSpace(destinationLocationCode) ||
                    string.IsNullOrWhiteSpace(departureDate))
                {
                    _logger.LogWarning("Invalid search parameters for flight offers");
                    return null;
                }

                // Retrieve the bearer token using the auth service
                var token = await _authService.GetAccessTokenAsync();
                if (token == null)
                {
                    _logger.LogError("Failed to retrieve Amadeus API token");
                    return null;
                }

                // Set the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Build the full request URL with parameters
                var requestParams = new List<string>
                {
                    $"originLocationCode={originLocationCode}",
                    $"destinationLocationCode={destinationLocationCode}",
                    $"departureDate={departureDate}",
                    $"adults={adults}",
                    $"max={max}",
                    $"currencyCode={currencyCode}"
                };

                // Add optional parameters if provided
                if (!string.IsNullOrWhiteSpace(returnDate))
                {
                    requestParams.Add($"returnDate={returnDate}");
                }

                if (!string.IsNullOrWhiteSpace(travelClass) && travelClass != "ECONOMY")
                {
                    requestParams.Add($"travelClass={travelClass}");
                }

                if (nonStop)
                {
                    requestParams.Add("nonStop=true");
                }

                string requestUrl = $"{_baseUrl}?{string.Join("&", requestParams)}";

                _logger.LogInformation($"Requesting flights: {requestUrl}");

                // Make the API call
                var response = await _httpClient.GetAsync(requestUrl);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error: {response.StatusCode}, {content}");
                    return null;
                }

                // Read and deserialize the response
                _logger.LogDebug($"Received flight data: {content.Substring(0, Math.Min(200, content.Length))}...");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var flightOffers = JsonSerializer.Deserialize<FlightOfferRootModel>(content, options);

                if (flightOffers == null)
                {
                    _logger.LogError("Failed to deserialize flight offers JSON response");
                    return null;
                }

                if (flightOffers.Data == null || flightOffers.Data.Count == 0)
                {
                    _logger.LogInformation($"No flight offers found for {originLocationCode} to {destinationLocationCode}");
                    // Return empty model instead of null to avoid warnings
                    return new FlightOfferRootModel { Data = new List<FlightOffer>() };
                }

                _logger.LogInformation($"Successfully retrieved {flightOffers.Data.Count} flight offers");
                return flightOffers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in GetFlightOffersAsync: {ex.Message}");
                return null;
            }
        }
    }
}

