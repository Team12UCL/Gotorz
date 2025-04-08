using Shared.Models;
using System.Text.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using Shared.Models.AirportRootModel;

namespace Server.Services
{
    public class AirportService
    {
        private readonly HttpClient _httpClient;
        private readonly AmadeusAuthService _authService;
        private readonly string _baseUrl;
        private readonly string _jsonFilePath = "Data/airports.json";
        private List<Location> Airports = new();

        public AirportService(IHttpClientFactory httpClientFactory, AmadeusAuthService authService, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("AmadeusClient");
            _authService = authService;
            _baseUrl = configuration["AmadeusAPI:AirportAndCitySearchUrl"]!;
            Task.Run(InitializeAirportsAsync).Wait(); // Load existing data at startup
        }

        private async Task InitializeAirportsAsync()
        {
            if (File.Exists(_jsonFilePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(_jsonFilePath);
                    Airports = JsonSerializer.Deserialize<List<Location>>(json) ?? new List<Location>();
                    Debug.WriteLine($"Loaded {Airports.Count} airports from JSON.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading airports: {ex.Message}");
                }
            }
        }

        private async Task SaveAirportsToJsonAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(Airports, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_jsonFilePath, json);
                Debug.WriteLine("Airports saved to JSON.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving airports: {ex.Message}");
            }
        }

        public async Task<List<Location>> PersistAirportsToJsonAsync(string cityOrAirportIATA)
        {
            try
            {
                Debug.WriteLine("Airports before: " + Airports.Count());

                if (string.IsNullOrWhiteSpace(cityOrAirportIATA)) return new List<Location>();

                var token = await _authService.GetAccessTokenAsync();
                if (token == null) return new List<Location>();

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Debug.WriteLine(token);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string requestUrl = $"{_baseUrl}?keyword={Uri.EscapeDataString(cityOrAirportIATA)}&subType=AIRPORT";
                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode) return new List<Location>();

                var content = await response.Content.ReadAsStringAsync();
                var rootObject = JsonSerializer.Deserialize<AirportRootModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (rootObject?.Data == null || rootObject.Data.Count == 0) return new List<Location>();

                foreach (var location in rootObject.Data)
                {
                    if (!Airports.Any(a => a.IataCode == location.IataCode))
                    {
                        Airports.Add(location);
                    }
                }

                Debug.WriteLine("Airports after: " + Airports.Count());

                await SaveAirportsToJsonAsync();
                return rootObject.Data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PersistAirportsToJsonAsync: {ex.Message}");
                return new List<Location>();
            }
        }

        public async Task<List<Location>> SearchAirportsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<Location>();

            return Airports
                .Where(a => a.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            a.IataCode.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            a.Address.CityName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            a.Address.CountryName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .ToList();
        }
    }
}