using System;
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
}