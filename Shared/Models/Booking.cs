
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
	public class Booking
	{
		public Guid BookingId { get; set; }
		public string UserId { get; set; }
		public Guid TravelPackageId { get; set; }
		public TravelPackage TravelPackage { get; set; }
		public DateTime BookingDate { get; set; }
		public DateTime TravelStartDate { get; set; }
		public DateTime TravelEndDate { get; set; }
		public string BookingReference { get; set; }
		public BookingStatus Status { get; set; }
		public PaymentStatus PaymentStatus { get; set; }
		public decimal TotalAmount { get; set; }
		public List<Passenger> Passengers { get; set; } = new List<Passenger>();
		public ContactInformation ContactInfo { get; set; }
		public List<BookingNote> Notes { get; set; } = new List<BookingNote>();
		public List<BookingAddon> Addons { get; set; } = new List<BookingAddon>();

	}

	public class Passenger
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string PassportNumber { get; set; }
		public string PassportCountry { get; set; }
		public DateTime PassportExpiryDate { get; set; }
		public bool IsLeadPassenger { get; set; }
		public string SpecialRequirements { get; set; }
	}

	public class ContactInformation
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string Country { get; set; }
		public string PostalCode { get; set; }
	}

	public class BookingNote
	{
		public int Id { get; set; }
		public string Content { get; set; }
		public string CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; }
		public bool IsInternal { get; set; } // Whether the note is visible to customers
	}

	public class BookingAddon
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public bool IsSelected { get; set; }
	}

	public enum BookingStatus
	{
		Pending,
		Confirmed,
		Cancelled,
		Completed,
		Refunded
	}

}
