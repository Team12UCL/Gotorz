using Shared.Models;
using System.Text.Json;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace Server.Services
{
    public class AirportService
    {
        private readonly HttpClient _httpClient;
        private readonly AmadeusAuthService _authService;
        private readonly string _baseUrl;
        private readonly string _jsonFilePath = "Data/airports.json";
        private List<Airport> Airports = new();
        private readonly ILogger<AirportService> _logger;

        public AirportService(IHttpClientFactory httpClientFactory, AmadeusAuthService authService, IConfiguration configuration, ILogger<AirportService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("AmadeusClient");
            _authService = authService;
            _baseUrl = configuration["AmadeusAPI:AirportAndCitySearchUrl"]!;
            _logger = logger;
            Task.Run(InitializeAirportsAsync).Wait();
        }

        private async Task InitializeAirportsAsync()
        {
            if (File.Exists(_jsonFilePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(_jsonFilePath);
                    Airports = JsonSerializer.Deserialize<List<Airport>>(json) ?? new List<Airport>();
                    Debug.WriteLine($"Loaded {Airports.Count} airports from JSON.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading airports: {ex.Message}");
                    Airports = new List<Airport>();
                }
            }
            else
            {
                Debug.WriteLine($"Airport data file does not exist. Creating an empty airport list.");
                Airports = new List<Airport>();

                // Create the Data directory if it doesn't exist
                var dataDirectory = Path.GetDirectoryName(_jsonFilePath);
                if (!string.IsNullOrEmpty(dataDirectory) && !Directory.Exists(dataDirectory))
                {
                    Directory.CreateDirectory(dataDirectory);
                }

                // Create an empty JSON file
                await SaveAirportsToJsonAsync();
            }
        }

        private async Task SaveAirportsToJsonAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(Airports);
                await File.WriteAllTextAsync(_jsonFilePath, json);
                Debug.WriteLine($"Saved {Airports.Count} airports to JSON.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving airports: {ex.Message}");
            }
        }

        public async Task<List<Airport>> PersistAirportsToJsonAsync(string cityOrAirportIATA)
        {
            try
            {
                _logger.LogInformation($"Airports before: {Airports.Count()}");

                if (string.IsNullOrWhiteSpace(cityOrAirportIATA)) return new List<Airport>();

                var token = await _authService.GetAccessTokenAsync();
                if (token == null) return new List<Airport>();

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug($"Using token: {token.Substring(0, Math.Min(10, token.Length))}...");

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Clean up the query string to avoid problematic characters
                string cleanQuery = cityOrAirportIATA.Trim();
                if (cleanQuery.StartsWith("-"))
                {
                    cleanQuery = cleanQuery.TrimStart('-').Trim();
                }

                string requestUrl = $"{_baseUrl}?keyword={Uri.EscapeDataString(cleanQuery)}&subType=AIRPORT,CITY&limit=10";
                _logger.LogInformation($"Requesting airports/cities: {requestUrl}");

                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error fetching airports: {response.StatusCode}. Response: {errorContent}");
                    return new List<Airport>();
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Received airport data: {content.Substring(0, Math.Min(200, content.Length))}...");

                var model = JsonSerializer.Deserialize<AirportRootModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (model?.Data == null || !model.Data.Any())
                {
                    _logger.LogWarning("No airports found in the response.");
                    return new List<Airport>();
                }

                // Add new items to the airports collection
                foreach (var airport in model.Data)
                {
                    if (!Airports.Any(a => a.IataCode == airport.IataCode))
                    {
                        Airports.Add(airport);
                    }
                }

                await SaveAirportsToJsonAsync();
                _logger.LogInformation($"Airports after: {Airports.Count()}");

                return model.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in PersistAirportsToJsonAsync: {ex.Message}");
                return new List<Airport>();
            }
        }

        public async Task<List<Airport>> SearchAirportsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<Airport>();

            // Only one Where clause needed
            var localResults = Airports
                .Where(a => (a.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                           (a.IataCode?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                           (a.CityName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                           (a.CountryName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .Take(5)
                .ToList();

            // If no results are found locally, try fetching from the API
            if (!localResults.Any())
            {
                _logger.LogInformation($"No local results for '{query}', fetching from API...");
                return await PersistAirportsToJsonAsync(query);
            }

            return localResults;
        }
    }
}