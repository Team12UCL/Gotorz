using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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

        // Enhanced search method with advanced options
        public async Task<FlightOfferRootModel> SearchFlightsAdvanced(FlightSearchRequest request)
        {
            try
            {
                _logger.LogInformation($"Performing advanced flight search from {request.OriginCode} to {request.DestinationCode}");

                var token = await _authService.GetAccessToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Build query parameters
                var queryParams = new Dictionary<string, string>
                {
                    ["originLocationCode"] = request.OriginCode,
                    ["destinationLocationCode"] = request.DestinationCode,
                    ["departureDate"] = request.DepartureDate.ToString("yyyy-MM-dd")
                };

                // Add optional parameters
                if (request.ReturnDate.HasValue)
                    queryParams["returnDate"] = request.ReturnDate.Value.ToString("yyyy-MM-dd");

                queryParams["adults"] = request.Adults.ToString();

                if (request.Children > 0)
                    queryParams["children"] = request.Children.ToString();

                if (request.Infants > 0)
                    queryParams["infants"] = request.Infants.ToString();

                if (!string.IsNullOrEmpty(request.TravelClass))
                    queryParams["travelClass"] = request.TravelClass;

                if (!string.IsNullOrEmpty(request.CurrencyCode))
                    queryParams["currencyCode"] = request.CurrencyCode;

                if (request.MaxResults > 0)
                    queryParams["max"] = request.MaxResults.ToString();

                if (!string.IsNullOrEmpty(request.IncludedAirlineCodes))
                    queryParams["includedAirlineCodes"] = request.IncludedAirlineCodes;

                if (!string.IsNullOrEmpty(request.ExcludedAirlineCodes))
                    queryParams["excludedAirlineCodes"] = request.ExcludedAirlineCodes;

                if (request.NonStop.HasValue)
                    queryParams["nonStop"] = request.NonStop.Value.ToString().ToLower();

                if (request.MaxPrice.HasValue)
                    queryParams["maxPrice"] = request.MaxPrice.Value.ToString();

                // Build URL with query string
                var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
                var url = $"https://test.api.amadeus.com/v2/shopping/flight-offers?{queryString}";

                _logger.LogInformation($"Making request to Amadeus API: {url}");

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error searching flights. Status: {response.StatusCode}, Content: {errorContent}");
                    throw new HttpRequestException($"Error searching flights. Status: {response.StatusCode}, Content: {errorContent}");
                }

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
                _logger.LogError(ex, "Error performing advanced flight search");
                throw;
            }
        }

        // Method to search flights with a POST request using the flight-offers/pricing endpoint
        public async Task<FlightOfferRootModel> SearchFlightsWithPricing(FlightSearchRequest request)
        {
            try
            {
                _logger.LogInformation($"Searching flights with pricing from {request.OriginCode} to {request.DestinationCode}");

                var token = await _authService.GetAccessToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Create the request body according to Amadeus API specifications
                var requestBody = new
                {
                    currencyCode = request.CurrencyCode ?? "USD",
                    originDestinations = new[]
                    {
                        new
                        {
                            id = "1",
                            originLocationCode = request.OriginCode,
                            destinationLocationCode = request.DestinationCode,
                            departureDateTimeRange = new
                            {
                                date = request.DepartureDate.ToString("yyyy-MM-dd")
                            }
                        }
                    },
                    travelers = BuildTravelers(request.Adults, request.Children, request.Infants),
                    sources = new[] { "GDS" },
                    searchCriteria = new
                    {
                        maxFlightOffers = request.MaxResults > 0 ? request.MaxResults : 10,
                        flightFilters = new
                        {
                            cabinRestrictions = new[]
                            {
                                new
                                {
                                    cabin = request.TravelClass ?? "ECONOMY",
                                    coverage = "MOST_SEGMENTS",
                                    originDestinationIds = new[] { "1" }
                                }
                            }
                        }
                    }
                };

                // Add return flight if it's a round trip
                if (request.ReturnDate.HasValue)
                {
                    var originDestinationsWithReturn = new[]
                    {
                        new
                        {
                            id = "1",
                            originLocationCode = request.OriginCode,
                            destinationLocationCode = request.DestinationCode,
                            departureDateTimeRange = new
                            {
                                date = request.DepartureDate.ToString("yyyy-MM-dd")
                            }
                        },
                        new
                        {
                            id = "2",
                            originLocationCode = request.DestinationCode,
                            destinationLocationCode = request.OriginCode,
                            departureDateTimeRange = new
                            {
                                date = request.ReturnDate.Value.ToString("yyyy-MM-dd")
                            }
                        }
                    };

                    requestBody = new
                    {
                        currencyCode = request.CurrencyCode ?? "USD",
                        originDestinations = originDestinationsWithReturn,
                        travelers = BuildTravelers(request.Adults, request.Children, request.Infants),
                        sources = new[] { "GDS" },
                        searchCriteria = new
                        {
                            maxFlightOffers = request.MaxResults > 0 ? request.MaxResults : 10,
                            flightFilters = new
                            {
                                cabinRestrictions = new[]
                                {
                                    new
                                    {
                                        cabin = request.TravelClass ?? "ECONOMY",
                                        coverage = "MOST_SEGMENTS",
                                        originDestinationIds = new[] { "1", "2" }
                                    }
                                }
                            }
                        }
                    };
                }

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(requestBody, jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"Making POST request to Amadeus Flight Offers Search API with body: {json}");

                var response = await _httpClient.PostAsync("https://test.api.amadeus.com/v2/shopping/flight-offers", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error searching flights. Status: {response.StatusCode}, Content: {errorContent}");
                    throw new HttpRequestException($"Error searching flights. Status: {response.StatusCode}, Content: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Received flight search response: {responseContent}");

                var deserializeOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var flightRoot = JsonSerializer.Deserialize<FlightOfferRootModel>(responseContent, deserializeOptions);

                if (flightRoot == null)
                {
                    throw new JsonException("Failed to deserialize flight offers response");
                }

                return flightRoot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing flight search with pricing");
                throw;
            }
        }

        private object[] BuildTravelers(int adults, int children, int infants)
        {
            var travelers = new List<object>();

            // Add adult travelers
            for (int i = 0; i < adults; i++)
            {
                travelers.Add(new
                {
                    id = (i + 1).ToString(),
                    travelerType = "ADULT"
                });
            }

            // Add child travelers
            for (int i = 0; i < children; i++)
            {
                travelers.Add(new
                {
                    id = (adults + i + 1).ToString(),
                    travelerType = "CHILD"
                });
            }

            // Add infant travelers
            for (int i = 0; i < infants; i++)
            {
                travelers.Add(new
                {
                    id = (adults + children + i + 1).ToString(),
                    travelerType = "INFANT"
                });
            }

            return travelers.ToArray();
        }
    }
}