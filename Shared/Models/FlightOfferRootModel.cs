using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
	public class FlightOfferRootModel
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public Meta Meta { get; set; }

		[InverseProperty(nameof(FlightOffer.FlightOfferRootModel))]
		public ICollection<FlightOffer> Data { get; set; } = new List<FlightOffer>();
	}

	public class Meta
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public int Count { get; set; }
		public Links Links { get; set; }
	}

	public class Links
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string Self { get; set; }
	}

	public class FlightOffer
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string Type { get; set; }
		public string Source { get; set; }
		public bool InstantTicketingRequired { get; set; }
		public bool NonHomogeneous { get; set; }
		public bool OneWay { get; set; }
		public bool IsUpsellOffer { get; set; }
		public string LastTicketingDate { get; set; }
		public DateTime LastTicketingDateTime { get; set; }
		public int NumberOfBookableSeats { get; set; }

		[ForeignKey(nameof(FlightOfferRootModel))]
		public Guid FlightOfferRootModelId { get; set; }

		public FlightOfferRootModel FlightOfferRootModel { get; set; }

		public ICollection<Itinerary> Itineraries { get; set; } = new List<Itinerary>();
		public Price Price { get; set; }
		public PricingOptions PricingOptions { get; set; }
		public ICollection<TravelerPricing> TravelerPricings { get; set; } = new List<TravelerPricing>();
	}

	public class Itinerary
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string Duration { get; set; }

		[InverseProperty(nameof(Segment.Itinerary))]
		public ICollection<Segment> Segments { get; set; } = new List<Segment>();
	}

	public class Segment
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public Departure Departure { get; set; }
		public Arrival Arrival { get; set; }
		public string CarrierCode { get; set; }
		public string Number { get; set; }
		public Aircraft Aircraft { get; set; }
		public Operating Operating { get; set; }
		public string Duration { get; set; }
		public int NumberOfStops { get; set; }
		public bool BlacklistedInEU { get; set; }

		[ForeignKey(nameof(Itinerary))]
		public Guid ItineraryId { get; set; }

		public Itinerary Itinerary { get; set; }
	}

	public class Departure
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string IataCode { get; set; }
		public string Terminal { get; set; }
		public DateTime At { get; set; }
	}

	public class Arrival
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string IataCode { get; set; }
		public DateTime At { get; set; }
		public string Terminal { get; set; }
	}

	public class Aircraft
    {
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Code { get; set; }
    }

    public class Operating
    {
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();
		public string CarrierCode { get; set; }
    }

	public class Price
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string Currency { get; set; }
		public string Total { get; set; }
		public string Base { get; set; }

		[InverseProperty(nameof(AdditionalService.Price))]
		public ICollection<AdditionalService> AdditionalServices { get; set; } = new List<AdditionalService>();

		[InverseProperty(nameof(Fee.Price))]
		public ICollection<Fee> Fees { get; set; } = new List<Fee>();

		public string GrandTotal { get; set; }
	}

	public class Fee
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string Amount { get; set; }
		public string Type { get; set; }

		[ForeignKey(nameof(Price))]
		public Guid PriceId { get; set; }

		public Price Price { get; set; }
	}

	public class AdditionalService
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string Amount { get; set; }
		public string Type { get; set; }

		[ForeignKey(nameof(Price))]
		public Guid PriceId { get; set; }

		public Price Price { get; set; }
	}

	public class PricingOptions
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public ICollection<string> FareType { get; set; } = new List<string>();
		public bool IncludedCheckedBagsOnly { get; set; }
	}

	public class TravelerPricing
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string FareOption { get; set; }
		public string TravelerType { get; set; }
		public Price Price { get; set; }

		[InverseProperty(nameof(FareDetailsBySegment.TravelerPricing))]
		public ICollection<FareDetailsBySegment> FareDetailsBySegments { get; set; } = new List<FareDetailsBySegment>();
	}

	public class FareDetailsBySegment
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string Cabin { get; set; }
		public string FareBasis { get; set; }
		public string BrandedFare { get; set; }
		public string BrandedFareLabel { get; set; }
		public string Class { get; set; }

		// Navigation property for checked bags
		public IncludedBags IncludedCheckedBags { get; set; }

		// Navigation property for cabin bags
		public IncludedBags IncludedCabinBags { get; set; }

		[ForeignKey(nameof(TravelerPricing))]
		public Guid TravelerPricingId { get; set; }

		public TravelerPricing TravelerPricing { get; set; }
	}

	public class IncludedBags
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public int Quantity { get; set; }

		// Foreign key for checked bags relationship
		public Guid? CheckedBagsFareDetailsBySegmentId { get; set; }

		// Foreign key for cabin bags relationship
		public Guid? CabinBagsFareDetailsBySegmentId { get; set; }

		// Navigation property for checked bags relationship
		[ForeignKey(nameof(CheckedBagsFareDetailsBySegmentId))]
		public FareDetailsBySegment CheckedBags { get; set; }

		// Navigation property for cabin bags relationship
		[ForeignKey(nameof(CabinBagsFareDetailsBySegmentId))]
		public FareDetailsBySegment CabinBags { get; set; }
	}



	public class Amenity
    {
		[Key]
		public Guid TravelerId { get; set; } = Guid.NewGuid();
		public string Description { get; set; }
        public bool IsChargeable { get; set; }
        public string AmenityType { get; set; }
        public AmenityProvider AmenityProvider { get; set; }
    }

    public class AmenityProvider
    {
		[Key]
		public Guid TravelerId { get; set; } = Guid.NewGuid();
		public string Name { get; set; }
    }
}

