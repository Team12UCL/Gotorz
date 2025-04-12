using Microsoft.AspNetCore.Mvc;
using Server.Services;
using System.Threading.Tasks;

namespace Gotorz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmadeusStatusController : ControllerBase
    {
        private readonly AmadeusAuthService _authService;
        private readonly ILogger<AmadeusStatusController> _logger;

        public AmadeusStatusController(AmadeusAuthService authService, ILogger<AmadeusStatusController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // GET: api/amadeusstatus/check
        [HttpGet("check")]
        public async Task<IActionResult> CheckAmadeusStatus()
        {
            _logger.LogInformation("Checking Amadeus API status");

            // Try to get a token to verify credentials are correct
            var token = await _authService.GetAccessTokenAsync();

            if (token == null)
            {
                _logger.LogWarning("Amadeus API credentials are invalid or not properly configured");
                return Ok(new
                {
                    status = "error",
                    message = "Amadeus API credentials are invalid or not properly configured. Please check your API Key and Secret in appsettings.json."
                });
            }

            _logger.LogInformation("Amadeus API credentials are valid");
            return Ok(new
            {
                status = "success",
                message = "Amadeus API credentials are valid and properly configured."
            });
        }
    }
}