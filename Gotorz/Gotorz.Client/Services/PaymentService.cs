using Shared.Models;
using System;
using System.Threading.Tasks;

namespace Gotorz.Client.Services
{
    public class PaymentService
    {
        // Simulate payment processing with a mock Stripe integration
        public async Task<PaymentResult> ProcessPayment(PaymentRequest request)
        {
            // Simulate API call delay
            await Task.Delay(1500);

            // Mock successful payment 90% of the time for demo purposes
            var isSuccessful = new Random().Next(1, 10) <= 9;

            return new PaymentResult
            {
                Success = isSuccessful,
                TransactionId = isSuccessful ? $"STRIPE-{Guid.NewGuid():N}" : null,
                ErrorMessage = isSuccessful ? null : "Your card was declined. Please try a different payment method.",
                ProcessedAt = DateTime.Now
            };
        }

        // Simulate saving a payment method
        public async Task<bool> SavePaymentMethod(string userId, PaymentMethod paymentMethod)
        {
            // Simulate API call delay
            await Task.Delay(500);

            // Mock success - in a real app this would save to a database
            return true;
        }

        // Get saved payment methods for a user
        public async Task<PaymentMethod[]> GetSavedPaymentMethods(string userId)
        {
            // Simulate API call delay
            await Task.Delay(500);

            // Return mock saved payment methods
            return new[]
            {
                new PaymentMethod
                {
                    Id = "pm_1",
                    CardType = "Visa",
                    Last4 = "4242",
                    ExpiryMonth = 12,
                    ExpiryYear = 2025,
                    IsDefault = true
                },
                new PaymentMethod
                {
                    Id = "pm_2",
                    CardType = "Mastercard",
                    Last4 = "9876",
                    ExpiryMonth = 3,
                    ExpiryYear = 2024,
                    IsDefault = false
                }
            };
        }
    }
}