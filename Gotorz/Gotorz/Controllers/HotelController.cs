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

    }
}
