using Microsoft.AspNetCore.Mvc;
using Server.Services;
using Shared.Models;

namespace Gotorz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirportController : ControllerBase
    {
        private readonly AirportService _airportService;

        public AirportController(AirportService airportService)
        {
            _airportService = airportService;
        }

        // GET: api/airport/search
        [HttpGet("suggest-airports")]
        public async Task<ActionResult<List<Airport>>> SuggestAirports([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query is required.");
            }

            var airports = await _airportService.SearchAirportsAsync(query);
            return Ok(airports);
        }
    }
}