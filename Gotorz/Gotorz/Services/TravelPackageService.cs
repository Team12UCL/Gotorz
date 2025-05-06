using Gotorz.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Shared.Models.DTO;

namespace Gotorz.Services
{
    public interface ITravelPackageService
    {
        Task<TravelPackage> CreateAsync(TravelPackage package);
        Task<TravelPackage?> GetByIdAsync(Guid id);
        Task<List<TravelPackage>> GetAllAsync(int skip = 0, int take = 10);
        Task<TravelPackage> UpdateAsync(TravelPackage package);
        Task DeleteAsync(Guid id);
        Task<List<TravelPackage>> GetByStatusAsync(TravelPackageStatus status);
    }

    public class TravelPackageService : ITravelPackageService
    {
        private readonly ApplicationDbContext _context;

        public TravelPackageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TravelPackage> CreateAsync(TravelPackage package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

			// Check if the outbound flight, return flight, and hotel are already in the database
			//var existingOutboundFlight = await _context.FlightOffer
			//	.FirstOrDefaultAsync(f => f.OfferId == package.OutboundFlightId.ToString());

			//if (existingOutboundFlight == null)
			//{
			//	_context.FlightOffer.Add(package.OutboundFlight);
			//}
   //         if (package.ReturnFlightId != null)
   //         {

			//	var existingReturnFlight = await _context.FlightOffer
			//	.FirstOrDefaultAsync(f => f.OfferId == package.ReturnFlightId.ToString());

			//	if (existingReturnFlight == null)
			//	{
			//		_context.FlightOffer.Add(package.ReturnFlight);
			//	}
			//}

			//var existingHotel = await _context.Hotel
			//	.FirstOrDefaultAsync(h => h.ExternalHotelId == package.HotelId.ToString());
			//if (existingHotel == null)
			//{
			//	_context.Hotel.Add(package.Hotel);
			//}


			// convert the package to a DTO
			//var TavelPackageDTO = new TravelPackageDTO()
   //         {
			//	TravelPackageId = package.TravelPackageId,
			//	OutboundFlightId = package.OutboundFlightId,
			//	ReturnFlightId = package.ReturnFlightId,
			//	HotelId = package.Hotel.Id,
			//	DepartureDate = package.DepartureDate,
			//	ReturnDate = package.ReturnDate,
			//	Adults = package.Adults,
			//	OriginCity = package.OriginCity,
			//	DestinationCity = package.DestinationCity,
			//	Name = package.Name,
			//	Description = package.Description,
			//	Status = package.Status
			//};


			await _context.TravelPackages.AddAsync(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<TravelPackage?> GetByIdAsync(Guid id)
        {
            return await _context.TravelPackages
                .Include(tp => tp.OutboundFlightId)
                .Include(tp => tp.ReturnFlightId)
                .Include(tp => tp.HotelId)
                .FirstOrDefaultAsync(tp => tp.TravelPackageId == id);
        }

        public async Task<List<TravelPackage>> GetAllAsync(int skip = 0, int take = 10)
        {
            return await _context.TravelPackages
                .Include(tp => tp.OutboundFlight)
                .Include(tp => tp.ReturnFlight)
                .Include(tp => tp.Hotel)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<TravelPackage> UpdateAsync(TravelPackage package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            var existingPackage = await _context.TravelPackages
                .FirstOrDefaultAsync(tp => tp.TravelPackageId == package.TravelPackageId);

            if (existingPackage == null)
                throw new KeyNotFoundException($"Travel package with ID {package.TravelPackageId} not found.");

            _context.Entry(existingPackage).CurrentValues.SetValues(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task DeleteAsync(Guid id)
        {
            var package = await _context.TravelPackages.FindAsync(id);
            if (package == null)
                throw new KeyNotFoundException($"Travel package with ID {id} not found.");

            _context.TravelPackages.Remove(package);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TravelPackage>> GetByStatusAsync(TravelPackageStatus status)
        {
            return await _context.TravelPackages
                .Include(tp => tp.OutboundFlightId)
                .Include(tp => tp.ReturnFlightId)
                .Include(tp => tp.HotelId)
                .Where(tp => tp.Status == status)
                .ToListAsync();
        }
    }
}
