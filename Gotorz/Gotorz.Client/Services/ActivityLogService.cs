using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gotorz.Client.Services
{
    public class ActivityLogService
    {
        private static readonly List<ActivityLog> _activityLogs = new List<ActivityLog>();

        // Log user activity
        public void LogActivity(string userId, string action, string details)
        {
            var log = new ActivityLog
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Action = action,
                Details = details,
                IpAddress = "127.0.0.1", // Mock IP address
                UserAgent = "Mozilla/5.0 Mock Browser",
                Timestamp = DateTime.Now
            };

            _activityLogs.Add(log);
            Console.WriteLine($"[Activity Log] User {userId}: {action} - {details}");
        }

        // Get all activity logs for admin review
        public List<ActivityLog> GetAllLogs(int limit = 100)
        {
            return _activityLogs.OrderByDescending(l => l.Timestamp).Take(limit).ToList();
        }

        // Get logs for a specific user
        public List<ActivityLog> GetUserLogs(string userId, int limit = 50)
        {
            return _activityLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToList();
        }

        // Export logs as CSV - mock implementation
        public async Task<string> ExportLogsAsCsv(DateTime from, DateTime to)
        {
            // Simulate processing delay
            await Task.Delay(1000);

            // In a real implementation, this would generate a CSV file
            return "mock_exported_logs.csv";
        }
    }

    public class ActivityLog
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
    }
}