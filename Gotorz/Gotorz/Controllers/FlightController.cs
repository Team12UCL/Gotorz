using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        private readonly FlightService _flightService;

        public FlightController(FlightService flightService)
        {
            _flightService = flightService;
        }

        // GET: api/flight/search
        [HttpGet("search")]
        public async Task<IActionResult> GetFlightOffers(
            [FromQuery] string originLocationCode,
            [FromQuery] string destinationLocationCode,
            [FromQuery] string departureDate,
            [FromQuery] int adults = 1)
        {
            var flightOffers = await _flightService.GetFlightOffersAsync(
                originLocationCode,
                destinationLocationCode,
                departureDate,
                adults);

            if (flightOffers == null)
            {
                return BadRequest("Could not fetch flight offers.");
            }
            return Ok(flightOffers);
        }
    }
}
