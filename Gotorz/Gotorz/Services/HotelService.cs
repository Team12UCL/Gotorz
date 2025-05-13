using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Shared.Models;
using Shared.Models.AmadeusCityResponse;

namespace Server.Services
{
    public class HotelService
    {
        private readonly HttpClient _httpClient;
        private readonly AmadeusAuthService _authService;
        private readonly string _hotelOffersBaseUrl;
        private readonly string _hotelsByCityBaseUrl;

        public HotelService(IHttpClientFactory httpClientFactory, AmadeusAuthService authService, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient("AmadeusClient");
            _authService = authService;
            _hotelOffersBaseUrl = config["AmadeusAPI:HotelOffersUrl"]!;
            _hotelsByCityBaseUrl = config["AmadeusAPI:HotelsByCityUrl"]!;
        }

        public async Task<List<Hotel>?> GetHotelOffersAsync(string cityCode, string checkInDate, string checkOutDate, int adults)
        {
            try
            {
                //Authorization 
                var token = await _authService.GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                //Get Hotel-ID's based on citycode
                var hotelIds = await GetHotelIdsByCityAsync(cityCode);

                if (hotelIds == null || hotelIds.Count == 0)
                {
                    return null;
                }

                var hotelIdsParam = string.Join(",", hotelIds.Take(20));

                var url = $"{_hotelOffersBaseUrl}?hotelIds={hotelIdsParam}&adults={adults}&checkInDate={checkInDate}&checkOutDate={checkOutDate}&currency=EUR";

                //Get data from Amadeus API
                var response = await _httpClient.GetAsync(url);

				if (!response.IsSuccessStatusCode)
				{
					return null;
				}
				
				var content = await response.Content.ReadAsStringAsync();
				var root = JsonDocument.Parse(content).RootElement;

                // Extract the conversion rate using a helper method
                decimal conversionRate = await GetCurrencyConversionRate(root);

                Debug.WriteLine($"🏨 Response: {content}");

				var hotelData = root.GetProperty("data");
				var hotels = new List<Hotel>();

				foreach (var hotelJson in hotelData.EnumerateArray())
				{
					if (!hotelJson.TryGetProperty("hotel", out var hotelInfo)) continue;

					var hotel = new Hotel
					{
						ExternalHotelId = hotelInfo.TryGetProperty("hotelId", out var hotelId) ? hotelId.GetString() : null,
						Name = hotelInfo.TryGetProperty("name", out var name) ? name.GetString() : null,
						CityCode = hotelInfo.TryGetProperty("cityCode", out var code) ? code.GetString() : null,
						Latitude = hotelInfo.TryGetProperty("latitude", out var lat) ? lat.GetDouble() : 0.0,
						Longitude = hotelInfo.TryGetProperty("longitude", out var lng) ? lng.GetDouble() : 0.0,
						Offers = new List<HotelOffer>()
					};

					if (hotelJson.TryGetProperty("offers", out var offersJson))
					{
						foreach (var offerJson in offersJson.EnumerateArray())
						{
							var room = offerJson.TryGetProperty("room", out var roomJson) ? roomJson : default;
							var roomEst = room.TryGetProperty("typeEstimated", out var roomEstJson) ? roomEstJson : default;
							var price = offerJson.TryGetProperty("price", out var priceJson) ? priceJson : default;
							var policies = offerJson.TryGetProperty("policies", out var policiesJson) ? policiesJson : default;

							hotel.Offers.Add(new HotelOffer
							{
								OfferId = offerJson.TryGetProperty("id", out var idJson) ? idJson.GetString() : Guid.NewGuid().ToString(),
								HotelDbId = hotel.Id,
								CheckInDate = offerJson.TryGetProperty("checkInDate", out var checkIn) ? DateTime.Parse(checkIn.GetString()) : DateTime.MinValue,
								CheckOutDate = offerJson.TryGetProperty("checkOutDate", out var checkOut) ? DateTime.Parse(checkOut.GetString()) : DateTime.MinValue,
								RoomType = room.TryGetProperty("type", out var type) ? type.GetString() : null,
								RoomCategory = roomEst.TryGetProperty("category", out var cat) ? cat.GetString() : null,
								BedType = roomEst.TryGetProperty("bedType", out var bedType) ? bedType.GetString() : null,
								NumberOfBeds = roomEst.TryGetProperty("beds", out var beds) ? beds.GetInt32() : 0,
								Description = room.TryGetProperty("description", out var desc) && desc.TryGetProperty("text", out var descText) ? descText.GetString() : null,
                                BasePrice = price.TryGetProperty("base", out var basePrice) && decimal.TryParse(basePrice.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var baseVal) ? baseVal : 0,
                                TotalPrice = price.TryGetProperty("total", out var totalPrice) && decimal.TryParse(totalPrice.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var totalVal) ? totalVal : 0,
                                Currency = price.TryGetProperty("currency", out var currency) ? currency.GetString() : null,
                                ConversionRate = conversionRate,
                                CancellationPolicy = policies.TryGetProperty("cancellations", out var cancelArr) && cancelArr.GetArrayLength() > 0 &&
													 cancelArr[0].TryGetProperty("description", out var cancelDesc) &&
													 cancelDesc.TryGetProperty("text", out var cancelText)
													 ? cancelText.GetString()
													 : "Unknown"
							});
						}
					}

                    Debug.WriteLine("The currency conversion rate is " + conversionRate);

					hotels.Add(hotel);
				}

				if (hotels.Count == 0)
				{
					Console.WriteLine($"🏨 No hotels found for {cityCode}");
					return null;
				}
				return hotels;
			}
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return null;
            }
        }

        private async Task<decimal> GetCurrencyConversionRate(JsonElement root)
        {
            var rate = root
                .GetProperty("dictionaries")
                .GetProperty("currencyConversionLookupRates")
                .EnumerateObject()
                .First()
                .Value
                .GetProperty("rate")
                .GetString();

            return decimal.Parse(rate, CultureInfo.InvariantCulture);
        }

        public async Task<List<CityData>> GetCitySuggestionsAsync(string query)
        {
            var token = await _authService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"https://test.api.amadeus.com/v1/reference-data/locations/cities?keyword={query}&max=10";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<CityData>();

            var result = await response.Content.ReadFromJsonAsync<AmadeusCityResponse>();
            return result?.Data ?? new List<CityData>();
        }

        private async Task<List<string>> GetHotelIdsByCityAsync(string cityCode)
        {
            try
            {
                var url = $"{_hotelsByCityBaseUrl}?cityCode={cityCode}";
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new List<string>();
                }

                var json = JsonDocument.Parse(content);
                var hotelIds = json.RootElement
                    .GetProperty("data")
                    .EnumerateArray()
                    .Select(h => h.GetProperty("hotelId").GetString())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToList();

                return hotelIds;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }
        public async Task<string?> GetCityCodeAsync(string cityName)
        {
            var token = await _authService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"https://test.api.amadeus.com/v1/reference-data/locations?subType=CITY&keyword={cityName}&page[limit]=1";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("data")
                .EnumerateArray()
                .FirstOrDefault()
                .GetProperty("iataCode")
                .GetString();
        }
    }
}
