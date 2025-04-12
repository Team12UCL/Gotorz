using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Gotorz.Services
{
    public class FlightService
    {
        private readonly HttpClient _httpClient;
        private readonly AmadeusAuthService _authService;
        private readonly ILogger<FlightService> _logger;

        public FlightService(HttpClient httpClient, AmadeusAuthService authService, ILogger<FlightService> logger)
        {
            _httpClient = httpClient;
            _authService = authService;
            _logger = logger;
        }

        public async Task<FlightOfferRootModel> SearchFlights(string originCode, string destinationCode,
            DateTime departureDate, DateTime returnDate, int adults = 1, string travelClass = "ECONOMY")
        {
            try
            {
                _logger.LogInformation($"Searching flights from {originCode} to {destinationCode}, " +
                                      $"departure: {departureDate:yyyy-MM-dd}, return: {returnDate:yyyy-MM-dd}");

                var token = await _authService.GetAccessToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var formattedDeparture = departureDate.ToString("yyyy-MM-dd");
                var formattedReturn = returnDate.ToString("yyyy-MM-dd");

                var url = $"https://test.api.amadeus.com/v2/shopping/flight-offers?" +
                          $"originLocationCode={originCode}&" +
                          $"destinationLocationCode={destinationCode}&" +
                          $"departureDate={formattedDeparture}&" +
                          $"returnDate={formattedReturn}&" +
                          $"adults={adults}&" +
                          $"travelClass={travelClass}&" +
                          $"currencyCode=USD&max=10";

                _logger.LogInformation($"Making request to Amadeus API: {url}");

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error searching flights. Status: {response.StatusCode}, Content: {errorContent}");
                    throw new HttpRequestException($"Error searching flights. Status: {response.StatusCode}, Content: {errorContent}");
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Received flight search response: {content}");

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var flightRoot = JsonSerializer.Deserialize<FlightOfferRootModel>(content, jsonOptions);

                if (flightRoot == null)
                {
                    throw new JsonException("Failed to deserialize flight offers response");
                }

                return flightRoot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flights");
                throw;
            }
        }
    }
}