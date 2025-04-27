using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gotorz.Data;
using Shared.Models;

namespace Gotorz.Services
{
	public class ActivityLogService
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly ILogger<ActivityLogService> _logger;

		public ActivityLogService(ApplicationDbContext dbContext, ILogger<ActivityLogService> logger)
		{
			_dbContext = dbContext;
			_logger = logger;
		}

		public async Task<bool> LogActivityAsync(ActivityLog activityLog)
		{
			try
			{
				await _dbContext.ActivityLogs.AddAsync(activityLog);
				await _dbContext.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error logging activity for user {activityLog.UserId}");
				return false;
			}
		}

		public async Task<bool> LogActivityAsync(string userId, string activityType, string description, string? ipAddress = null)
		{
			var activityLog = new ActivityLog
			{
				UserId = userId,
				ActivityType = activityType,
				Description = description,
				Timestamp = DateTime.UtcNow,
				IpAddress = ipAddress ?? string.Empty
			};

			return await LogActivityAsync(activityLog);
		}

		public async Task<List<ActivityLog>> GetUserActivityLogsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, int skip = 0, int take = 50)
		{
			try
			{
				var query = _dbContext.ActivityLogs
					.Where(a => a.UserId == userId);

				if (startDate.HasValue)
				{
					query = query.Where(a => a.Timestamp >= startDate.Value);
				}

				if (endDate.HasValue)
				{
					query = query.Where(a => a.Timestamp <= endDate.Value);
				}

				return await query
					.OrderByDescending(a => a.Timestamp)
					.Skip(skip)
					.Take(take)
					.ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error retrieving activity logs for user {userId}");
				return new List<ActivityLog>();
			}
		}

		public async Task<List<ActivityLog>> GetAllActivityLogsAsync(
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
				var query = _dbContext.ActivityLogs.AsQueryable();

				if (!string.IsNullOrEmpty(activityType))
				{
					query = query.Where(a => a.ActivityType == activityType);
				}

				if (startDate.HasValue)
				{
					query = query.Where(a => a.Timestamp >= startDate.Value);
				}

				if (endDate.HasValue)
				{
					query = query.Where(a => a.Timestamp <= endDate.Value);
				}

				if (!string.IsNullOrEmpty(ipAddress))
				{
					query = query.Where(a => a.IpAddress.Contains(ipAddress));
				}

				if (!string.IsNullOrEmpty(userAgent))
				{
					query = query.Where(a => a.UserAgent.Contains(userAgent));
				}

				if (!string.IsNullOrEmpty(resourceType))
				{
					query = query.Where(a => a.ResourceType.Contains(resourceType));
				}

				return await query
					.OrderByDescending(a => a.Timestamp)
					.Skip(skip)
					.Take(take)
					.ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving activity logs");
				throw;
			}
		}

		public async Task<byte[]> ExportActivityLogsAsCsvAsync(
			string? userId = null, 
			string? activityType = null, 
			DateTime? startDate = null, 
			DateTime? endDate = null,
			string? ipAddress = null,
			string? resourceType = null)
		{
			try
			{
				var query = _dbContext.ActivityLogs.AsQueryable();

				if (!string.IsNullOrEmpty(userId))
				{
					query = query.Where(a => a.UserId == userId);
				}

				if (!string.IsNullOrEmpty(activityType))
				{
					query = query.Where(a => a.ActivityType == activityType);
				}

				if (startDate.HasValue)
				{
					query = query.Where(a => a.Timestamp >= startDate.Value);
				}

				if (endDate.HasValue)
				{
					query = query.Where(a => a.Timestamp <= endDate.Value);
				}

				if (!string.IsNullOrEmpty(ipAddress))
				{
					query = query.Where(a => a.IpAddress.Contains(ipAddress));
				}

				if (!string.IsNullOrEmpty(resourceType))
				{
					query = query.Where(a => a.ResourceType.Contains(resourceType));
				}

				var logs = await query
					.OrderByDescending(a => a.Timestamp)
					.ToListAsync();

				// Generate CSV
				using (var memoryStream = new System.IO.MemoryStream())
				using (var writer = new System.IO.StreamWriter(memoryStream))
				{
					// Write header
					writer.WriteLine("ID,UserID,ActivityType,Description,Timestamp,IPAddress,UserAgent,ResourceType,ResourceId");

					// Write data rows
					foreach (var log in logs)
					{
						writer.WriteLine($"{log.Id},{log.UserId},{log.ActivityType},{EscapeCsvField(log.Description)},{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.IpAddress},{EscapeCsvField(log.UserAgent)},{log.ResourceType},{log.ResourceId}");
					}

					writer.Flush();
					return memoryStream.ToArray();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error exporting activity logs to CSV");
				throw;
			}
		}

		private string EscapeCsvField(string field)
		{
			if (string.IsNullOrEmpty(field))
				return string.Empty;

			if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
			{
				// Replace double quotes with double double quotes
				field = field.Replace("\"", "\"\"");
				// Wrap in quotes
				return $"\"{field}\"";
			}

			return field;
		}
	}
}