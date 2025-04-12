using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gotorz.Client.Services
{
    public class TravelPackageService
    {
        private static List<TravelPackage> _packages = new();
        private static List<Booking> _bookings = new();
        private static int _nextBookingId = 1000;

        public List<TravelPackage> Packages
        {
            get => _packages;
            set => _packages = value;
        }

        public List<Booking> Bookings => _bookings;

        // Create a booking from a package
        public Booking CreateBooking(TravelPackage package, string userId)
        {
            var booking = new Booking
            {
                Id = _nextBookingId++,
                UserId = userId,
                Package = package,
                BookingDate = DateTime.Now,
                TravelStartDate = package.DepartureDate,
                TravelEndDate = package.ReturnDate,
                Status = BookingStatus.Confirmed,
                PaymentStatus = PaymentStatus.Paid,
                ReferenceNumber = GenerateReferenceNumber(),
                TotalAmount = CalculateTotalAmount(package)
            };

            _bookings.Add(booking);
            return booking;
        }

        // Get bookings for a specific user
        public List<Booking> GetUserBookings(string userId)
        {
            return _bookings.Where(b => b.UserId == userId).ToList();
        }

        // Generate a random reference number for the booking
        private string GenerateReferenceNumber()
        {
            return $"GDT-{DateTime.Now:yyMMdd}-{new Random().Next(1000, 9999)}";
        }

        // Calculate the total amount for the package
        private decimal CalculateTotalAmount(TravelPackage package)
        {
            decimal total = 0;

            // Add flight costs
            if (package.OutboundFlight != null && decimal.TryParse(package.OutboundFlight.Price?.Total, out decimal outboundPrice))
            {
                total += outboundPrice;
            }

            if (package.ReturnFlight != null && decimal.TryParse(package.ReturnFlight.Price?.Total, out decimal returnPrice))
            {
                total += returnPrice;
            }

            // Add hotel cost
            if (package.Hotel?.Offers?.FirstOrDefault()?.Price != null &&
                decimal.TryParse(package.Hotel.Offers.FirstOrDefault().Price.Total, out decimal hotelPrice))
            {
                total += hotelPrice;
            }

            // Add service fee (5%)
            total += total * 0.05m;

            return Math.Round(total, 2);
        }
    }
}
