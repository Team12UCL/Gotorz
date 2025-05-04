
using Shared.Models;


namespace Gotorz.Services.Admin
{
    public class AdminActivityLogService
    {
        private readonly ActivityLogService _activityLogService;

        public AdminActivityLogService(ActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        public async Task<List<ActivityLog>> GetActivityLogsAsync(
            string? activityType = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            int skip = 0, 
            int take = 100,
            string? ipAddress = null,
            string? userAgent = null,
            string? resourceType = null)
        {
            try
            {
                return await _activityLogService.GetAllActivityLogsAsync(
                    activityType, startDate, endDate, skip, take, ipAddress, userAgent, resourceType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching activity logs", ex);
            }
        }

        public async Task<byte[]> ExportActivityLogsAsync(
            string? userId = null, 
            string? activityType = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null,
            string? ipAddress = null,
            string? resourceType = null)
        {
            try
            {
                return await _activityLogService.ExportActivityLogsAsCsvAsync(
                    userId, activityType, startDate, endDate, ipAddress, resourceType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error exporting activity logs", ex);
            }
        }
    }
}
