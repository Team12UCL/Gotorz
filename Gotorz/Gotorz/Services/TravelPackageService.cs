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
