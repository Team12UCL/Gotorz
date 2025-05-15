using Shared.Models;
using System.Text.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Server.Services
{
    public class AirportService
    {
        private List<Airport> _airports;
        // Local statis list of airports for airport suggestions
        private readonly string _jsonFilePath = "Data/airports.json";

        public AirportService()
        {
            LoadAirportsFromJson();
        }

        public AirportService(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath;
            LoadAirportsFromJson();
        }

        private void LoadAirportsFromJson()
        {
            try
            {
                string jsonContent = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var allLocations = JsonSerializer.Deserialize<List<Airport>>(jsonContent, options);

                // The JSON also contains railway stations, so filter out anything that isn't an airport via the type value
                _airports = allLocations
                    .Where(location => string.Equals(location.Type, "Airports", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading airports: {ex.Message}");
                _airports = new List<Airport>();
            }
        }

        public async Task<List<Airport>> SearchAirportsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Airport>();

            return await Task.FromResult(_airports
                .Where(a => a.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           a.IataCode.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           a.City.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           a.Country.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList());
        }
    }
}