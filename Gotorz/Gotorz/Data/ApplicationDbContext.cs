using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System.Reflection.Emit;

namespace Gotorz.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //public DbSet<Booking> Bookings { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; } = default!;
        public DbSet<FlightOffer> FlightOffer { get; set; }
        public DbSet<Itinerary> Itineraries { get; set; }
        public DbSet<FlightSegment> FlightSegments { get; set; }
        public DbSet<Hotel> Hotel { get; set; }
        public DbSet<HotelOffer> HotelOffer { get; set; }
        public DbSet<TravelPackage> TravelPackages { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);




            builder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserName).IsRequired();
                entity.Property(e => e.Text).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
            });

            // Hotel → HotelOffer (1-to-many)
            builder.Entity<Hotel>()
                .HasMany(h => h.Offers)
                .WithOne()
                .HasForeignKey(ho => ho.HotelDbId)
                .OnDelete(DeleteBehavior.Cascade);

            // FlightOffer → Itineraries (1-to-many)
            builder.Entity<FlightOffer>()
    .HasMany(f => f.Itineraries)
    .WithOne()
    .HasForeignKey(i => i.FlightOfferId)
    .OnDelete(DeleteBehavior.Cascade);

            // Itinerary → FlightSegments (1-to-many)
            builder.Entity<Itinerary>()
                .HasMany(i => i.Segments)
                .WithOne()
                .HasForeignKey(s => s.ItineraryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TravelPackage>(entity =>
            {
                entity.HasKey(tp => tp.TravelPackageId);


                entity.HasOne(tp => tp.OutboundFlight)
                    .WithMany()
                    .HasForeignKey(tp => tp.OutboundFlightId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tp => tp.ReturnFlight)
                    .WithMany()
                    .HasForeignKey(tp => tp.ReturnFlightId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tp => tp.Hotel)
                    .WithMany()
                    .HasForeignKey(tp => tp.HotelId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}