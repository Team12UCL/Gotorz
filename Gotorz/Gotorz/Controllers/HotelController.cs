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
        private readonly ILogger<HotelController> _logger;

        public HotelController(HotelService hotelService, ILogger<HotelController> logger)
        {
            _hotelService = hotelService;
            _logger = logger;
        }

        // GET: api/hotel/search?cityCode=PAR
        [HttpGet("search")]
        public async Task<IActionResult> GetHotelOffers([FromQuery] string cityCode,
                                                        [FromQuery] string departureDate,
                                                        [FromQuery] int adults = 1)
        {
            _logger.LogInformation($"Fetching hotel offers for city: {cityCode}, date: {departureDate}, adults: {adults}");

            if (string.IsNullOrWhiteSpace(departureDate))
                departureDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

            var hotelOffers = await _hotelService.GetHotelOffersAsync(cityCode, departureDate, adults);

            if (hotelOffers == null)
            {
                return BadRequest("Could not fetch hotel offers.");
            }

            return Ok(hotelOffers);
        }

    }
}
