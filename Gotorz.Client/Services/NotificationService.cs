using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gotorz.Client.Services
{
    public class NotificationService
    {
        private readonly List<Notification> _notifications = new();

        public NotificationService()
        {
            // Initialize with some sample notifications
            AddSampleNotifications();
        }

        private void AddSampleNotifications()
        {
            _notifications.Add(new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "user1",
                Type = NotificationType.BookingConfirmation,
                Message = "Your booking to Paris has been confirmed",
                Link = "/bookings/details/GDT-230412-1234",
                CreatedAt = DateTime.Now.AddDays(-1),
                IsRead = false
            });

            _notifications.Add(new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "user1",
                Type = NotificationType.PaymentConfirmation,
                Message = "Payment of $1,250.00 was successful",
                Link = "/payments/details/trx_12345678",
                CreatedAt = DateTime.Now.AddDays(-1),
                IsRead = true
            });

            _notifications.Add(new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "user1",
                Type = NotificationType.TravelReminder,
                Message = "Your trip to Paris is just 30 days away!",
                Link = "/bookings/details/GDT-230412-1234",
                CreatedAt = DateTime.Now.AddHours(-5),
                IsRead = false
            });
        }

        // Get all notifications for a user
        public List<Notification> GetNotificationsForUser(string userId)
        {
            return _notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        // Get unread notifications count
        public int GetUnreadNotificationsCount(string userId)
        {
            return _notifications
                .Count(n => n.UserId == userId && !n.IsRead);
        }

        // Mark a notification as read
        public void MarkNotificationAsRead(string notificationId)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
            }
        }

        // Mark all notifications as read for a user
        public void MarkAllNotificationsAsRead(string userId)
        {
            foreach (var notification in _notifications.Where(n => n.UserId == userId))
            {
                notification.IsRead = true;
            }
        }

        // Add a new notification
        public Notification AddNotification(string userId, NotificationType type, string message, string link = null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Type = type,
                Message = message,
                Link = link,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _notifications.Add(notification);

            return notification;
        }

        // Delete a notification
        public bool DeleteNotification(string notificationId)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                return _notifications.Remove(notification);
            }
            return false;
        }

        // Clear all notifications for a user
        public void ClearAllNotifications(string userId)
        {
            _notifications.RemoveAll(n => n.UserId == userId);
        }
    }
}