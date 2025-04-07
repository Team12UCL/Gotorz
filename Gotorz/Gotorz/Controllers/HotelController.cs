using Microsoft.AspNetCore.Mvc;
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

        // GET: api/hotel/search?cityCode=PAR
        [HttpGet("search")]
        public async Task<IActionResult> GetHotelOffers([FromQuery] string cityCode,
                                                        [FromQuery] string departureDate,
                                                        [FromQuery] int adults = 1)
        {
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
