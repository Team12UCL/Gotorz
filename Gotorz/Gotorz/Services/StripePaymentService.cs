using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gotorz.Services
{
    public class StripePaymentService
    {
        private readonly string _apiKey;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
        {
            _apiKey = configuration["Stripe:SecretKey"] ?? throw new ArgumentNullException("Stripe:SecretKey must be configured");
            StripeConfiguration.ApiKey = _apiKey;
            _logger = logger;
        }

        public async Task<PaymentResult> ProcessPayment(PaymentRequest request)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100), // Convert to cents
                    Currency = request.Currency.ToLower(),
                    Description = request.Description,
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingReference", request.BookingReference },
                        { "UserId", request.UserId }
                    },
                    ReceiptEmail = request.Email
                };

                // Handle payment method
                if (!string.IsNullOrEmpty(request.PaymentMethodId))
                {
                    // Use saved payment method
                    options.PaymentMethod = request.PaymentMethodId;
                }
                else if (request.CardDetails != null)
                {
                    // Create new payment method from card details
                    var cardOptions = new PaymentMethodCreateOptions
                    {
                        Type = "card",
                        Card = new PaymentMethodCardOptions
                        {
                            Number = request.CardDetails.Number,
                            ExpMonth = request.CardDetails.ExpiryMonth,
                            ExpYear = request.CardDetails.ExpiryYear,
                            Cvc = request.CardDetails.Cvv
                        },
                        BillingDetails = new PaymentMethodBillingDetailsOptions
                        {
                            Name = request.CardDetails.HolderName,
                            Address = new AddressOptions
                            {
                                PostalCode = request.CardDetails.PostalCode
                            }
                        }
                    };

                    var paymentMethodService = new PaymentMethodService();
                    var paymentMethod = await paymentMethodService.CreateAsync(cardOptions);
                    options.PaymentMethod = paymentMethod.Id;

                    // Save payment method if requested
                    if (request.SavePaymentMethod && !string.IsNullOrEmpty(request.UserId))
                    {
                        await SavePaymentMethod(request.UserId, paymentMethod.Id);
                    }
                }
                else
                {
                    throw new ArgumentException("Either PaymentMethodId or CardDetails must be provided");
                }

                options.Confirm = true;

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                var success = paymentIntent.Status == "succeeded";

                return new PaymentResult
                {
                    Success = success,
                    TransactionId = paymentIntent.Id,
                    ErrorMessage = success ? null : "Payment processing failed. Please try again.",
                    ProcessedAt = DateTime.UtcNow
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe payment processing error");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProcessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment processing error");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = "An unexpected error occurred while processing your payment.",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<bool> SavePaymentMethod(string userId, string paymentMethodId)
        {
            try
            {
                // In a real implementation, you would:
                // 1. Create or get a Stripe Customer for this user
                // 2. Attach the payment method to the customer
                // 3. Save the reference in your database

                var customerService = new CustomerService();
                var customer = await GetOrCreateCustomer(userId);

                var paymentMethodService = new PaymentMethodService();
                await paymentMethodService.AttachAsync(paymentMethodId, new PaymentMethodAttachOptions
                {
                    Customer = customer.Id,
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving payment method for user {userId}");
                return false;
            }
        }

        public async Task<List<PaymentMethod>> GetSavedPaymentMethods(string userId)
        {
            try
            {
                var customer = await GetOrCreateCustomer(userId);

                var paymentMethodService = new PaymentMethodService();
                var options = new PaymentMethodListOptions
                {
                    Customer = customer.Id,
                    Type = "card"
                };

                var paymentMethods = await paymentMethodService.ListAsync(options);
                var result = new List<PaymentMethod>();

                foreach (var pm in paymentMethods)
                {
                    result.Add(new PaymentMethod
                    {
                        Id = pm.Id,
                        CardType = pm.Card.Brand,
                        Last4 = pm.Card.Last4,
                        ExpiryMonth = pm.Card.ExpMonth.Value,
                        ExpiryYear = pm.Card.ExpYear.Value,
                        CardholderName = pm.BillingDetails?.Name,
                        IsDefault = pm.Id == customer.InvoiceSettings?.DefaultPaymentMethod
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving payment methods for user {userId}");
                return new List<PaymentMethod>();
            }
        }

        private async Task<Customer> GetOrCreateCustomer(string userId)
        {
            var customerService = new CustomerService();
            var listOptions = new CustomerListOptions
            {
                Limit = 1
            };
            listOptions.AddExtraParam("metadata[UserId]", userId);

            var customers = await customerService.ListAsync(listOptions);

            if (customers.Data.Count > 0)
            {
                return customers.Data[0];
            }

            // Customer doesn't exist, create one
            var createOptions = new CustomerCreateOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId }
                }
            };

            return await customerService.CreateAsync(createOptions);
        }
    }
}