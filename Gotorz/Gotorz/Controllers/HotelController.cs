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

        // GET: api/hotel/search?cityCode=PAR&checkInDate=2023-12-01&checkOutDate=2023-12-05&adults=2
        [HttpGet("search")]
        public async Task<IActionResult> GetHotelOffers(
            [FromQuery] string cityCode,
            [FromQuery] string checkInDate,
            [FromQuery] string checkOutDate,
            [FromQuery] int adults = 1)
        {
            _logger.LogInformation($"Searching hotels in {cityCode} from {checkInDate} to {checkOutDate} for {adults} adults");

            if (string.IsNullOrWhiteSpace(cityCode))
            {
                _logger.LogWarning("Missing cityCode in hotel search request");
                return BadRequest("City code is required.");
            }

            if (string.IsNullOrWhiteSpace(checkInDate) || string.IsNullOrWhiteSpace(checkOutDate))
            {
                _logger.LogWarning("Missing dates in hotel search request");
                return BadRequest("Check-in and check-out dates are required.");
            }

            var hotelOffers = await _hotelService.GetHotelOffersAsync(cityCode, checkInDate, checkOutDate, adults);

            if (hotelOffers == null)
            {
                _logger.LogWarning($"Failed to retrieve hotel offers for {cityCode}");
                return BadRequest("Could not fetch hotel offers.");
            }

            _logger.LogInformation($"Found {hotelOffers.Data?.Count ?? 0} hotel offers for {cityCode}");
            return Ok(hotelOffers);
        }
    }
}
