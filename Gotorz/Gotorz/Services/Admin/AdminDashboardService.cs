using Gotorz.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Gotorz.Services.Admin
{
	public class AdminDashboardService
	{
		private readonly ApplicationDbContext _dbContext;

		public AdminDashboardService(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<DashboardStats> GetDashboardStatsAsync()
		{
			var totalUsers = await _dbContext.Users.CountAsync();
			//var totalBookings = await _dbContext.Bookings.CountAsync();
			//var totalSales = await _dbContext.Bookings.SumAsync(b => (decimal?)b.TotalAmount) ?? 0m;
			//var recentBookings = await _dbContext.Bookings
			//	.OrderByDescending(b => b.BookingDate)
			//	.Take(5)
			//	.ToListAsync();
			var activeUsers = await _dbContext.ActivityLogs
				.Where(a => a.Timestamp >= DateTime.UtcNow.AddDays(-30))
				.Select(a => a.UserId)
				.Distinct()
				.CountAsync();

			return new DashboardStats
			{
				TotalUsers = totalUsers,
				TotalBookings = 0,
				TotalSales = 0,
				ActiveUsers = activeUsers,
				RecentBookings = new List<Booking>()
			};
		}
	}

	public class DashboardStats
	{
		public int TotalUsers { get; set; }
		public int TotalBookings { get; set; }
		public decimal TotalSales { get; set; }
		public int ActiveUsers { get; set; }
		public List<Booking> RecentBookings { get; set; }
	}
}
