using Microsoft.AspNetCore.Mvc;
using Server.Services;
using Shared.Models;
using Shared.Models.AirportRootModel;
using System.Diagnostics;

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

        // GET: api/airport/add-airports
        [HttpPost("add-airports")]
        public async Task<IActionResult> SearchAndAddAirports([FromBody] string query)
        {

            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query cannot be empty.");

            // Wait for the operation to complete
            await _airportService.PersistAirportsToJsonAsync(query);

            return Ok("Airports added.");
        }

        // GET: api/airport/search
        [HttpGet("suggest-airports")]
        public async Task<ActionResult<List<AirportRootModel>>> SuggestAirports([FromQuery] string query)
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