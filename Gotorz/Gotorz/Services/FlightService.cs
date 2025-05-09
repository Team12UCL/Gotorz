using Shared.Models;
using System.Diagnostics;
using System.Globalization;
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

        public async Task<List<FlightOffer>?> GetFlightOffersAsync(
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

                // Retrieve the bearer token for Amadeus using the auth service
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

                // Make the API call to Amadeus
                var response = await _httpClient.GetAsync(requestUrl);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Error: {response.StatusCode}, {errorContent}");
                    return null;
                }

                // Read the response
                var content = await response.Content.ReadAsStringAsync();
                var root = JsonDocument.Parse(content).RootElement;
                var flightOffers = new List<FlightOffer>();

                foreach (var offer in root.GetProperty("data").EnumerateArray())
                {
                    var flightOffer = new FlightOffer
                    {
                        OfferId = offer.GetProperty("id").GetString(),
                        AirlineCode = offer.GetProperty("validatingAirlineCodes")[0].GetString(),
                        TotalPrice = decimal.Parse(offer.GetProperty("price").GetProperty("total").GetString(),
                             NumberStyles.Number, CultureInfo.InvariantCulture),
                        BasePrice = decimal.Parse(offer.GetProperty("price").GetProperty("base").GetString(),
                            NumberStyles.Number, CultureInfo.InvariantCulture),
                        Currency = offer.GetProperty("price").GetProperty("currency").GetString(),
                        AvailableSeats = offer.GetProperty("numberOfBookableSeats").GetInt32(),
                        Itineraries = new List<Itinerary>()
                    };

                    foreach (var itineraryJson in offer.GetProperty("itineraries").EnumerateArray())
                    {
                        var itinerary = new Itinerary
                        {
                            Duration = itineraryJson.GetProperty("duration").GetString(),
                            Segments = new List<FlightSegment>()
                        };

                        foreach (var segment in itineraryJson.GetProperty("segments").EnumerateArray())
                        {
                            itinerary.Segments.Add(new FlightSegment
                            {
                                DepartureAirport = segment.GetProperty("departure").GetProperty("iataCode").GetString(),
                                DepartureTime = DateTime.Parse(segment.GetProperty("departure").GetProperty("at").GetString()),
                                ArrivalAirport = segment.GetProperty("arrival").GetProperty("iataCode").GetString(),
                                ArrivalTime = DateTime.Parse(segment.GetProperty("arrival").GetProperty("at").GetString()),
                                CarrierCode = segment.GetProperty("carrierCode").GetString(),
                                FlightNumber = segment.GetProperty("number").GetString(),
                                AircraftCode = segment.GetProperty("aircraft").GetProperty("code").GetString(),
                                Stops = segment.GetProperty("numberOfStops").GetInt32(),
                                CabinClass = "ECONOMY",
                                CheckedBags = 0
                            });
                        }

                        flightOffer.Itineraries.Add(itinerary);
                    }

                    flightOffers.Add(flightOffer);
                }

                if (flightOffers == null)
                {
                    Debug.WriteLine("Deserialization returned null.");
                    return null;
                }
                else
                {
                    Debug.WriteLine($"Successfully deserialized flight offers with {flightOffers.Count} items.");
                    return flightOffers;
                }
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

