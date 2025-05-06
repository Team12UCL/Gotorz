using Microsoft.EntityFrameworkCore;
using Gotorz.Data;
using Shared.Models;

namespace Gotorz.Services.Admin
{
	public class AdminBookingService
	{
		private readonly ApplicationDbContext _dbContext;

		public AdminBookingService(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		//public async Task<List<Booking>> GetBookingsAsync(int skip = 0, int take = 100)
		//{
		//	return await _dbContext.Bookings
		//		.Include(b => b.TravelPackage)
		//		.OrderByDescending(b => b.BookingDate)
		//		.Skip(skip)
		//		.Take(take)
		//		.ToListAsync();
		//}
	}
}
