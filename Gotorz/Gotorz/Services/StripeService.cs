using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gotorz.Services
{
    public interface IStripeService
    {
        Task<Session> CreateCheckoutSession(string productName, string description, long amountInCents, int quantity);
    }

    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly string _domain;

        public StripeService(IConfiguration configuration)
        {
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            _domain = _configuration["Stripe:Domain"];
        }

        public async Task<Session> CreateCheckoutSession(string productName, string description, long amountInCents, int quantity)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = amountInCents,
                            Currency = "eur",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = productName,
                                Description = description
                            }
                        },
                        Quantity = quantity
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{_domain}/stripetest/success",
                CancelUrl = $"{_domain}/stripetest/cancel",
            };

            var service = new SessionService();
            return await service.CreateAsync(options);
        }
    }
}