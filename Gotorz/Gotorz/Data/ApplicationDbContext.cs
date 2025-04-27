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

		public DbSet<Booking> Bookings { get; set; }
		public DbSet<ActivityLog> ActivityLogs { get; set; }
		public DbSet<ChatMessage> ChatMessages { get; set; } = default!;
		public DbSet<FlightOfferRootModel> FlightOfferRootModels { get; set; }
		public DbSet<FlightOffer> FlightOffers { get; set; }
		public DbSet<Itinerary> Itineraries { get; set; }
		public DbSet<Segment> Segments { get; set; }
		public DbSet<Price> Prices { get; set; }
		public DbSet<TravelerPricing> TravelerPricings { get; set; }
		public DbSet<FareDetailsBySegment> FareDetailsBySegments { get; set; }
		public DbSet<IncludedBags> IncludedBags { get; set; }
		public DbSet<AdditionalService> AdditionalServices { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

			builder.Entity<FlightOfferRootModel>()
				.HasMany(f => f.Data)
				.WithOne()
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<FlightOffer>()
	   .HasOne(f => f.FlightOfferRootModel)
	   .WithMany(r => r.Data)
	   .HasForeignKey(f => f.FlightOfferRootModelId)
	   .OnDelete(DeleteBehavior.Cascade); // Keep this as Cascade

			builder.Entity<Itinerary>()
				.HasMany(i => i.Segments)
				.WithOne()
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<Price>()
				.HasMany(p => p.Fees)
				.WithOne()
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<Price>()
				.HasMany(p => p.AdditionalServices)
				.WithOne()
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<TravelerPricing>()
				.HasMany(t => t.FareDetailsBySegments)
				.WithOne()
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<FareDetailsBySegment>()
		.HasOne(f => f.TravelerPricing)
		.WithMany(t => t.FareDetailsBySegments)
		.HasForeignKey(f => f.TravelerPricingId)
	
		.OnDelete(DeleteBehavior.Cascade);
			builder.Entity<FareDetailsBySegment>()
	   .HasOne(f => f.IncludedCheckedBags)
	   .WithOne(b => b.CheckedBags)
	   .HasForeignKey<IncludedBags>(b => b.CheckedBagsFareDetailsBySegmentId)
	   .OnDelete(DeleteBehavior.Restrict);

		
			builder.Entity<FareDetailsBySegment>()
				.HasOne(f => f.IncludedCabinBags)
				.WithOne(b => b.CabinBags)
				.HasForeignKey<IncludedBags>(b => b.CabinBagsFareDetailsBySegmentId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<AdditionalService>()
	   .HasOne(a => a.Price)
	   .WithMany(p => p.AdditionalServices)
	   .HasForeignKey(a => a.PriceId)
	   .OnDelete(DeleteBehavior.Cascade);

			builder.Entity<Fee>()
		.HasOne(f => f.Price)
		.WithMany(p => p.Fees)
		.HasForeignKey(f => f.PriceId)
		.OnDelete(DeleteBehavior.Cascade);
			builder.Entity<Segment>()
		.HasOne(s => s.Itinerary)
		.WithMany(i => i.Segments)
		.HasForeignKey(s => s.ItineraryId)
		.OnDelete(DeleteBehavior.Cascade);


			// Example configuration for ChatMessage
			builder.Entity<ChatMessage>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(e => e.UserName).IsRequired();
				entity.Property(e => e.Text).IsRequired();
				entity.Property(e => e.Timestamp).IsRequired();
			});
		}
    }
}