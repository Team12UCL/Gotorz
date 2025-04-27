using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
	public class Payment
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public Guid BookingId { get; set; }

		[Required]
		public string TransactionId { get; set; }

		public decimal Amount { get; set; }
		public string Currency { get; set; } = "USD";

		public string PaymentMethod { get; set; } // CreditCard, PayPal, BankTransfer
		public string Status { get; set; } // Pending, Completed, Failed, Refunded

		// For card payments
		public string CardType { get; set; }
		public string Last4Digits { get; set; }

		// PayPal specific
		public string PayPalEmail { get; set; }

		// Bank transfer specific
		public string BankName { get; set; }
		public string AccountReference { get; set; }

		// Receipt
		public string ReceiptUrl { get; set; }

		// Timestamps
		public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
		public DateTime? RefundDate { get; set; }
	}

	public class PaymentMethod
	{
		public string Id { get; set; }
		public string CardType { get; set; }
		public string Last4 { get; set; }
		public int ExpiryMonth { get; set; }
		public int ExpiryYear { get; set; }
		public bool IsDefault { get; set; }
		public string CardholderName { get; set; }
	}

	public class CardDetails
	{
		public string Number { get; set; }
		public string HolderName { get; set; }
		public int ExpiryMonth { get; set; }
		public int ExpiryYear { get; set; }
		public string Cvv { get; set; }
		public string PostalCode { get; set; }
	}

	public class PaymentRequest
	{
		public string Email { get; set; }
		public decimal Amount { get; set; }
		public string Currency { get; set; } = "USD";
		public string Description { get; set; }
		public string BookingReference { get; set; }
		public string PaymentMethodId { get; set; }
		public CardDetails CardDetails { get; set; }
		public string UserId { get; set; }
		public bool SavePaymentMethod { get; set; }
	}

	public class PaymentResult
	{
		public bool Success { get; set; }
		public string TransactionId { get; set; }
		public string ErrorMessage { get; set; }
		public DateTime ProcessedAt { get; set; }
	}

	public enum PaymentStatus
	{
		Pending,
		Paid,
		Refunded,
		Failed,
		Cancelled
	}
}