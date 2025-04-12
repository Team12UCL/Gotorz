using Microsoft.AspNetCore.Identity;

namespace Gotorz.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string PassportNumber { get; set; }
        public DateTime PassportExpiryDate { get; set; }
        public string PreferredLanguage { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public bool IsSubscribedToNewsletter { get; set; }
    }
}