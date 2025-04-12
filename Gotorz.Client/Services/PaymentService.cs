using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gotorz.Client.Services
{
    public class PaymentService
    {
        private static readonly List<PaymentMethod> _savedPaymentMethods = new();

        public PaymentService()
        {
            // Initialize with some test data if empty
            if (_savedPaymentMethods.Count == 0)
            {
                _savedPaymentMethods.Add(new PaymentMethod
                {
                    Id = "pm_visa_1234",
                    CardType = "Visa",
                    Last4 = "4242",
                    ExpiryMonth = 12,
                    ExpiryYear = 2025,
                    IsDefault = true,
                    CardholderName = "Test User"
                });

                _savedPaymentMethods.Add(new PaymentMethod
                {
                    Id = "pm_mastercard_5678",
                    CardType = "Mastercard",
                    Last4 = "5678",
                    ExpiryMonth = 6,
                    ExpiryYear = 2026,
                    IsDefault = false,
                    CardholderName = "Test User"
                });
            }
        }

        // Get saved payment methods for a user
        public List<PaymentMethod> GetSavedPaymentMethods(string userId)
        {
            // In a real app, this would filter by user ID
            return _savedPaymentMethods.ToList();
        }

        // Save a new payment method
        public PaymentMethod SavePaymentMethod(string userId, CardDetails cardDetails)
        {
            // In a real app, this would make an API call to a payment processor
            // to tokenize the card details securely

            // Create a payment method with masked data
            var paymentMethod = new PaymentMethod
            {
                Id = $"pm_{GetCardType(cardDetails.Number).ToLower()}_{new Random().Next(1000, 9999)}",
                CardType = GetCardType(cardDetails.Number),
                Last4 = cardDetails.Number.Substring(cardDetails.Number.Length - 4),
                ExpiryMonth = cardDetails.ExpiryMonth,
                ExpiryYear = cardDetails.ExpiryYear,
                IsDefault = _savedPaymentMethods.Count == 0, // First card becomes default
                CardholderName = cardDetails.HolderName
            };

            _savedPaymentMethods.Add(paymentMethod);

            return paymentMethod;
        }

        // Process a payment
        public async Task<PaymentResult> ProcessPayment(PaymentRequest request)
        {
            // Simulate payment processing delay
            await Task.Delay(2000);

            // In a real app, this would make an API call to a payment processor

            // For demo purposes, simulate a successful payment
            var isSuccess = true;
            var errorMessage = string.Empty;

            // Simulate some payment failures based on amount
            if (request.Amount > 10000)
            {
                isSuccess = false;
                errorMessage = "Transaction declined: Amount exceeds permitted limit";
            }

            // If using a saved payment method, validate it exists
            if (!string.IsNullOrEmpty(request.PaymentMethodId))
            {
                var paymentMethod = _savedPaymentMethods.FirstOrDefault(pm => pm.Id == request.PaymentMethodId);
                if (paymentMethod == null)
                {
                    isSuccess = false;
                    errorMessage = "Invalid payment method";
                }
            }

            // If new card details provided and user wants to save them
            if (request.CardDetails != null && request.SavePaymentMethod && isSuccess)
            {
                SavePaymentMethod(request.UserId, request.CardDetails);
            }

            return new PaymentResult
            {
                Success = isSuccess,
                TransactionId = isSuccess ? $"trx_{Guid.NewGuid().ToString().Substring(0, 8)}" : null,
                ErrorMessage = errorMessage,
                ProcessedAt = DateTime.Now
            };
        }

        // Issue a refund
        public async Task<PaymentResult> ProcessRefund(string transactionId, decimal amount, string reason)
        {
            // Simulate refund processing delay
            await Task.Delay(1500);

            // In a real app, this would make an API call to payment processor
            // to issue a refund for the transaction

            // For demo purposes, simulate a successful refund
            return new PaymentResult
            {
                Success = true,
                TransactionId = $"ref_{Guid.NewGuid().ToString().Substring(0, 8)}",
                ErrorMessage = null,
                ProcessedAt = DateTime.Now
            };
        }

        // Set a payment method as default
        public bool SetDefaultPaymentMethod(string paymentMethodId)
        {
            var paymentMethod = _savedPaymentMethods.FirstOrDefault(pm => pm.Id == paymentMethodId);
            if (paymentMethod == null)
            {
                return false;
            }

            // Clear default flag on all payment methods
            foreach (var pm in _savedPaymentMethods)
            {
                pm.IsDefault = false;
            }

            // Set new default
            paymentMethod.IsDefault = true;

            return true;
        }

        // Remove a payment method
        public bool RemovePaymentMethod(string paymentMethodId)
        {
            var paymentMethod = _savedPaymentMethods.FirstOrDefault(pm => pm.Id == paymentMethodId);
            if (paymentMethod == null)
            {
                return false;
            }

            _savedPaymentMethods.Remove(paymentMethod);

            // If removed was default and we have other payment methods, set a new default
            if (paymentMethod.IsDefault && _savedPaymentMethods.Count > 0)
            {
                _savedPaymentMethods[0].IsDefault = true;
            }

            return true;
        }

        // Helper method to determine card type from card number
        private string GetCardType(string cardNumber)
        {
            // Very simplified card type detection - in a real app use a proper library
            if (string.IsNullOrEmpty(cardNumber))
                return "Unknown";

            // Remove spaces and dashes
            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

            if (cardNumber.StartsWith("4"))
                return "Visa";
            else if (cardNumber.StartsWith("5"))
                return "Mastercard";
            else if (cardNumber.StartsWith("3"))
                return "Amex";
            else if (cardNumber.StartsWith("6"))
                return "Discover";
            else
                return "Unknown";
        }
    }
}