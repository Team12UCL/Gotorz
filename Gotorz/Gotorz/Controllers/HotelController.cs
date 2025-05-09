using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly HotelService _hotelService;

        public HotelController(HotelService hotelService)
        {
            _hotelService = hotelService;
        }

        [HttpGet("suggest-cities")]
        public async Task<IActionResult> SuggestCities([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query is required.");

            var cities = await _hotelService.GetCitySuggestionsAsync(query);

            if (cities == null || !cities.Any())
                return NotFound("No city suggestions found.");

            return Ok(cities);
        }

        // GET: api/hotel/search?cityCode=PAR
        [HttpGet("search")]
        public async Task<IActionResult> GetHotelOffers([FromQuery] string cityCode,
                                                        [FromQuery] string checkInDate,
                                                        [FromQuery] string checkOutDate,
                                                        [FromQuery] int adults = 1)
        {

            var hotelOffers = await _hotelService.GetHotelOffersAsync(cityCode, checkInDate, checkOutDate, adults);

            if (hotelOffers == null)
            {
                return BadRequest("Could not fetch hotel offers.");
            }

            return Ok(hotelOffers);
        }

        [HttpGet("get-city-code")]
        public async Task<IActionResult> GetCityCode([FromQuery] string cityName)
        {
            var result = await _hotelService.GetCityCodeAsync(cityName);
            if (string.IsNullOrWhiteSpace(result))
                return NotFound("City code not found");

            return Ok(new { cityCode = result });
        }

    }
}
