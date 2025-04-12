using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Gotorz.Services;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Gotorz.Services
{
    public class AirportService
    {
        private readonly HttpClient _httpClient;
        private readonly AmadeusAuthService _authService;
        private readonly ILogger<AirportService> _logger;
        private List<Airport> _cachedAirports;

        public AirportService(HttpClient httpClient, AmadeusAuthService authService, ILogger<AirportService> logger)
        {
            _httpClient = httpClient;
            _authService = authService;
            _logger = logger;
        }

        public async Task<List<Airport>> SearchAirports(string keyword)
        {
            if (_cachedAirports == null)
            {
                await LoadAirportsFromFile();
            }

            if (_cachedAirports != null)
            {
                // Search in the cached airports
                var result = new List<Airport>();
                var lowercaseKeyword = keyword.ToLower();

                foreach (var airport in _cachedAirports)
                {
                    if (airport.Name.ToLower().Contains(lowercaseKeyword) ||
                        airport.CityName.ToLower().Contains(lowercaseKeyword) ||
                        airport.CountryName.ToLower().Contains(lowercaseKeyword) ||
                        airport.IataCode.ToLower().Contains(lowercaseKeyword))
                    {
                        result.Add(airport);
                    }
                }

                return result;
            }

            // If there's no cached data, call the API
            var token = await _authService.GetAccessToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"https://test.api.amadeus.com/v1/reference-data/locations?subType=AIRPORT&keyword={keyword}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var airportRoot = JsonSerializer.Deserialize<AirportRootModel>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return airportRoot.Data;
        }

        private async Task LoadAirportsFromFile()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "airports.json");
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    var airportRoot = JsonSerializer.Deserialize<AirportRootModel>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    _cachedAirports = airportRoot.Data;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading airports from file");
                _cachedAirports = null;
            }
        }
    }
}