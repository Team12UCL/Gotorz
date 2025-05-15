using Gotorz.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Gotorz.Services
{
    public class TravelPackageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TravelPackageService> _logger;

        public TravelPackageService(ApplicationDbContext context, ILogger<TravelPackageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TravelPackage>> GetAllAsync(int skip = 0, int take = 10)
        {
            _logger.LogInformation($"Retrieving travel packages, skip: {skip}, take: {take}");

            var packages = await _context.TravelPackages
                .Include(p => p.OutboundFlight)
                    .ThenInclude(f => f.Itineraries)
                        .ThenInclude(i => i.Segments)
                .Include(p => p.ReturnFlight)
                    .ThenInclude(f => f.Itineraries)
                        .ThenInclude(i => i.Segments)
                .Include(p => p.Hotel)
                    .ThenInclude(h => h.Offers)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            _logger.LogInformation($"Retrieved {packages.Count()} travel packages");

            foreach (var package in packages)
            {
                if (package.Hotel != null && (package.Hotel.Offers == null || !package.Hotel.Offers.Any()))
                {
                    _logger.LogWarning($"Hotel ID {package.Hotel.Id} has no offers");
                }
            }

            return packages;
        }

        public async Task<TravelPackage> GetByIdAsync(Guid id)
        {
            _logger.LogInformation($"Retrieving travel package with ID: {id}");

            var package = await _context.TravelPackages
                .Include(p => p.OutboundFlight)
                    .ThenInclude(f => f.Itineraries)
                        .ThenInclude(i => i.Segments)
                .Include(p => p.ReturnFlight)
                    .ThenInclude(f => f.Itineraries)
                        .ThenInclude(i => i.Segments)
                .Include(p => p.Hotel)
                    .ThenInclude(h => h.Offers)
                .FirstOrDefaultAsync(p => p.TravelPackageId == id);

            if (package == null)
            {
                _logger.LogWarning($"Travel package with ID {id} not found");
                return null;
            }

            if (package.Hotel != null && (package.Hotel.Offers == null || !package.Hotel.Offers.Any()))
            {
                _logger.LogWarning($"Hotel ID {package.Hotel.Id} in package {id} has no offers");
            }

            return package;
        }

        public async Task<TravelPackage> CreateAsync(TravelPackage package)
        {
            _logger.LogInformation($"Creating travel package with hotel ID: {package.HotelId}");

            if (package.Hotel != null && package.HotelId != Guid.Empty)
            {
                var existingHotel = await _context.Hotel
                    .Include(h => h.Offers)
                    .FirstOrDefaultAsync(h => h.Id == package.HotelId);

                if (existingHotel != null)
                {
                    _logger.LogInformation($"Found existing hotel with ID {existingHotel.Id} with {existingHotel.Offers?.Count ?? 0} offers");

                    _context.Entry(package.Hotel).State = EntityState.Detached;
                    package.Hotel = existingHotel;
                }
                else
                {
                    _logger.LogWarning($"Hotel with ID {package.HotelId} not found in database. Creating new hotel.");

                    if (package.Hotel.Id == Guid.Empty)
                    {
                        package.Hotel.Id = package.HotelId;
                    }

                    _logger.LogInformation($"New hotel has {package.Hotel.Offers?.Count ?? 0} offers");
                }
            }

            await _context.TravelPackages.AddAsync(package);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created travel package with ID: {package.TravelPackageId}");

            return package;
        }

        public async Task<TravelPackage> UpdateAsync(TravelPackage package)
        {
            _logger.LogInformation($"Updating travel package with ID: {package.TravelPackageId}");

            var existingPackage = await _context.TravelPackages
                .Include(p => p.OutboundFlight)
                .Include(p => p.ReturnFlight)
                .Include(p => p.Hotel)
                    .ThenInclude(h => h.Offers)
                .FirstOrDefaultAsync(p => p.TravelPackageId == package.TravelPackageId);

            if (existingPackage == null)
            {
                _logger.LogWarning($"Travel package with ID {package.TravelPackageId} not found for update");
                throw new KeyNotFoundException($"TravelPackage with ID {package.TravelPackageId} not found.");
            }

            _context.Entry(existingPackage).CurrentValues.SetValues(package);

            if (package.HotelId != existingPackage.HotelId || package.Hotel != null)
            {
                if (package.Hotel != null)
                {
                    var existingHotel = await _context.Hotel
                        .Include(h => h.Offers)
                        .FirstOrDefaultAsync(h => h.Id == package.HotelId);

                    if (existingHotel != null)
                    {
                        existingPackage.Hotel = existingHotel;
                        existingPackage.HotelId = existingHotel.Id;
                    }
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Updated travel package with ID: {package.TravelPackageId}");

            return existingPackage;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation($"Deleting travel package with ID: {id}");

            var package = await _context.TravelPackages.FindAsync(id);
            if (package == null)
            {
                _logger.LogWarning($"Travel package with ID {id} not found for deletion");
                throw new KeyNotFoundException($"TravelPackage with ID {id} not found.");
            }

            var res = _context.TravelPackages.Remove(package);
            await _context.SaveChangesAsync();

            if (res.State == EntityState.Deleted)
            {
                return true;
                _logger.LogInformation($"Deleted travel package with ID: {id}");

            }
            else
            {
                return false;
            }
        }
    }
}
