using Shared.Models;
using System;
using System.Threading.Tasks;

namespace Gotorz.Client.Services
{
    public class NotificationService
    {
        // Simulate sending booking confirmation email
        public async Task<bool> SendBookingConfirmation(Booking booking, string userEmail)
        {
            // Simulate API delay
            await Task.Delay(1000);

            // In a real implementation, this would connect to an email service
            Console.WriteLine($"[Mock] Sending booking confirmation to {userEmail} for booking {booking.ReferenceNumber}");

            // Log this notification
            AddNotification(booking.UserId, NotificationType.BookingConfirmation,
                $"Your booking {booking.ReferenceNumber} has been confirmed.");

            return true;
        }

        // Simulate sending travel documents
        public async Task<bool> SendTravelDocuments(Booking booking, string userEmail)
        {
            // Simulate API delay
            await Task.Delay(1000);

            // In a real implementation, this would connect to an email service and generate PDFs
            Console.WriteLine($"[Mock] Sending travel documents to {userEmail} for booking {booking.ReferenceNumber}");

            // Log this notification
            AddNotification(booking.UserId, NotificationType.TravelDocuments,
                $"Your travel documents for booking {booking.ReferenceNumber} are ready.");

            return true;
        }

        // Simulate sending travel guide
        public async Task<bool> SendTravelGuide(Booking booking, string userEmail, string destination)
        {
            // Simulate API delay
            await Task.Delay(1000);

            // In a real implementation, this would connect to an email service and attach destination guide
            Console.WriteLine($"[Mock] Sending {destination} travel guide to {userEmail}");

            // Log this notification
            AddNotification(booking.UserId, NotificationType.TravelGuide,
                $"Your travel guide for {destination} has been sent to your email.");

            return true;
        }

        // Add a notification to the user's account
        private void AddNotification(string userId, NotificationType type, string message)
        {
            // In a real implementation, this would save to a database
            // For now, we'll just simulate the action
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Type = type,
                Message = message,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            Console.WriteLine($"[Mock] Added notification: {notification.Message}");
        }
    }
}