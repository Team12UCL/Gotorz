using System;
using System.Collections.Generic;
using System.Linq;

namespace Gotorz.Client.Services
{
    public class ActivityLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public ActivityType Type { get; set; }
        public string Description { get; set; }
        public string ResourceId { get; set; }
        public string ResourceType { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public object Metadata { get; set; }
    }

    public enum ActivityType
    {
        Login,
        Logout,
        PasswordChange,
        ProfileUpdate,
        Search,
        ViewPackage,
        BookingCreated,
        BookingModified,
        BookingCancelled,
        PaymentProcessed,
        RefundProcessed,
        ReviewSubmitted,
        SupportChatStarted,
        PasswordReset,
        AccountCreated,
        AccountDeleted,
        WishlistUpdated,
        DocumentDownloaded
    }

    public class ActivityLogService
    {
        private readonly List<ActivityLog> _activityLogs = new();

        public ActivityLogService()
        {
            // Add some sample logs for demo purposes
            AddSampleLogs();
        }

        private void AddSampleLogs()
        {
            // Sample login log
            _activityLogs.Add(new ActivityLog
            {
                UserId = "user1",
                Type = ActivityType.Login,
                Description = "User logged in",
                IpAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                Timestamp = DateTime.Now.AddDays(-1)
            });

            // Sample search log
            _activityLogs.Add(new ActivityLog
            {
                UserId = "user1",
                Type = ActivityType.Search,
                Description = "Searched for trips to Paris",
                ResourceType = "Search",
                Timestamp = DateTime.Now.AddDays(-1).AddHours(1),
                Metadata = new { SearchTerm = "Paris", Filters = "7 days, 2 adults" }
            });

            // Sample package view log
            _activityLogs.Add(new ActivityLog
            {
                UserId = "user1",
                Type = ActivityType.ViewPackage,
                Description = "Viewed Paris Getaway package",
                ResourceId = "pkg-12345",
                ResourceType = "TravelPackage",
                Timestamp = DateTime.Now.AddDays(-1).AddHours(1).AddMinutes(15)
            });

            // Sample booking log
            _activityLogs.Add(new ActivityLog
            {
                UserId = "user1",
                Type = ActivityType.BookingCreated,
                Description = "Created booking for Paris Getaway",
                ResourceId = "GDT-230412-1234",
                ResourceType = "Booking",
                Timestamp = DateTime.Now.AddDays(-1).AddHours(2),
                Metadata = new { PackageId = "pkg-12345", Amount = 1250.00, Currency = "USD" }
            });

            // Sample payment log
            _activityLogs.Add(new ActivityLog
            {
                UserId = "user1",
                Type = ActivityType.PaymentProcessed,
                Description = "Payment processed for booking GDT-230412-1234",
                ResourceId = "trx_12345678",
                ResourceType = "Payment",
                Timestamp = DateTime.Now.AddDays(-1).AddHours(2).AddMinutes(5),
                Metadata = new { BookingId = "GDT-230412-1234", Amount = 1250.00, Currency = "USD", PaymentMethod = "Credit Card" }
            });
        }

        // Log a new activity
        public ActivityLog LogActivity(
            string userId,
            ActivityType type,
            string description,
            string resourceId = null,
            string resourceType = null,
            object metadata = null)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                Type = type,
                Description = description,
                ResourceId = resourceId,
                ResourceType = resourceType,
                IpAddress = "127.0.0.1", // In a real app, this would be the actual IP
                UserAgent = "GodTur App", // In a real app, this would be the actual user agent
                Timestamp = DateTime.Now,
                Metadata = metadata
            };

            _activityLogs.Add(log);
            return log;
        }

        // Get activity logs for a user
        public List<ActivityLog> GetActivityLogsForUser(
            string userId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            ActivityType? activityType = null)
        {
            var query = _activityLogs.Where(log => log.UserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(log => log.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.Timestamp <= endDate.Value);
            }

            if (activityType.HasValue)
            {
                query = query.Where(log => log.Type == activityType.Value);
            }

            return query.OrderByDescending(log => log.Timestamp).ToList();
        }

        // Get activity logs for a specific resource
        public List<ActivityLog> GetActivityLogsForResource(
            string resourceId,
            string resourceType = null)
        {
            var query = _activityLogs.Where(log => log.ResourceId == resourceId);

            if (!string.IsNullOrEmpty(resourceType))
            {
                query = query.Where(log => log.ResourceType == resourceType);
            }

            return query.OrderByDescending(log => log.Timestamp).ToList();
        }

        // Get recent activity summary (e.g., for admin dashboard)
        public Dictionary<ActivityType, int> GetActivitySummary(DateTime startDate, DateTime endDate)
        {
            return _activityLogs
                .Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate)
                .GroupBy(log => log.Type)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );
        }

        // Clear logs older than a certain date (for data retention policies)
        public int ClearOldLogs(DateTime cutoffDate)
        {
            int count = _activityLogs.RemoveAll(log => log.Timestamp < cutoffDate);
            return count;
        }
    }
}