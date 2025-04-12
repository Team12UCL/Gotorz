using System;

namespace Shared.Models
{
    public enum NotificationType
    {
        BookingConfirmation,
        BookingChanged,
        BookingCancelled,
        PaymentConfirmation,
        Refund,
        TravelReminder,
        TravelDocuments,
        TravelGuide,
        PriceAlert,
        SystemNotification
    }

    public class Notification
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}