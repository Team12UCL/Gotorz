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

        public AirportService(IHttpClientFactory httpClientFactory, AmadeusAuthService authService, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("AmadeusClient");
            _authService = authService;
            _baseUrl = configuration["AmadeusAPI:AirportAndCitySearchUrl"]!;
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
                Debug.WriteLine("Airports before: " + Airports.Count());

                if (string.IsNullOrWhiteSpace(cityOrAirportIATA)) return new List<Airport>();

                var token = await _authService.GetAccessTokenAsync();
                if (token == null) return new List<Airport>();

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Debug.WriteLine("Using token: " + token.Substring(0, Math.Min(10, token.Length)) + "...");

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string requestUrl = $"{_baseUrl}?keyword={Uri.EscapeDataString(cityOrAirportIATA)}&subType=AIRPORT,CITY&limit=10";
                Debug.WriteLine($"Requesting airports/cities: {requestUrl}");

                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Error fetching airports: {response.StatusCode}");
                    return new List<Airport>();
                }

                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Received airport data: {content.Substring(0, Math.Min(200, content.Length))}...");

                var model = JsonSerializer.Deserialize<AirportRootModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (model?.Data == null || !model.Data.Any())
                {
                    Debug.WriteLine("No airports found in the response.");
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
                Debug.WriteLine("Airports after: " + Airports.Count());

                return model.Data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in PersistAirportsToJsonAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<Airport>();
            }
        }

        public async Task<List<Airport>> SearchAirportsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<Airport>();

            var localResults = Airports
                .Where(a => a.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           a.IataCode.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           a.CityName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           a.CountryName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .ToList();

            // If no results are found locally, try fetching from the API
            if (!localResults.Any())
            {
                Debug.WriteLine($"No local results for '{query}', fetching from API...");
                return await PersistAirportsToJsonAsync(query);
            }

            return localResults;
        }
    }
}