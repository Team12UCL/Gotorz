using Gotorz.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gotorz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly IStripeService _stripeService;

        public StripeController(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

        // GET: api/stripe/create-checkout-session
        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutRequest request)
        {
            var session = await _stripeService.CreateCheckoutSession(
                request.ProductName,
                request.Description,
                request.AmountInCents,
                request.Quantity);

            return Ok(new { url = session.Url });
        }
    }

    public class CheckoutRequest
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public long AmountInCents { get; set; }
        public int Quantity { get; set; } = 1;
    }
}