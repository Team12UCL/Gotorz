using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Gotorz.Client.Services
{
    public class LocalizationService
    {
        private Dictionary<string, Dictionary<string, string>> _translations = new();
        private string _currentLanguage = "en"; // Default language

        public LocalizationService()
        {
            // Initialize with English translations
            var englishTranslations = new Dictionary<string, string>
            {
                // Common
                ["Welcome"] = "Welcome",
                ["Search"] = "Search",
                ["Book"] = "Book",
                ["Cancel"] = "Cancel",
                ["Confirm"] = "Confirm",
                ["Login"] = "Login",
                ["Register"] = "Register",
                ["Logout"] = "Logout",
                ["Profile"] = "Profile",

                // Home page
                ["SearchForDestination"] = "Search for your dream destination",
                ["DepartureDate"] = "Departure date",
                ["ReturnDate"] = "Return date",
                ["Travelers"] = "Travelers",
                ["FindPackages"] = "Find Packages",

                // Booking
                ["BookingDetails"] = "Booking Details",
                ["PaymentInformation"] = "Payment Information",
                ["TravelerInformation"] = "Traveler Information",
                ["FlightInformation"] = "Flight Information",
                ["HotelInformation"] = "Hotel Information",
                ["CompleteBooking"] = "Complete Booking",

                // Order History
                ["MyBookings"] = "My Bookings",
                ["UpcomingTrips"] = "Upcoming Trips",
                ["PastTrips"] = "Past Trips",
                ["BookingReference"] = "Booking Reference",
                ["BookingDate"] = "Booking Date",
                ["TravelDates"] = "Travel Dates",
                ["Destination"] = "Destination",
                ["Status"] = "Status",
                ["TotalAmount"] = "Total Amount",

                // Chat
                ["ChatRooms"] = "Chat Rooms",
                ["CustomerSupport"] = "Customer Support",
                ["SendMessage"] = "Send Message",
                ["TypeMessage"] = "Type your message here...",

                // GDPR
                ["PersonalData"] = "Personal Data",
                ["DownloadData"] = "Download My Data",
                ["DeleteAccount"] = "Delete My Account",
                ["PrivacySettings"] = "Privacy Settings"
            };

            // Arabic translations
            var arabicTranslations = new Dictionary<string, string>
            {
                // Common
                ["Welcome"] = "مرحباً",
                ["Search"] = "بحث",
                ["Book"] = "حجز",
                ["Cancel"] = "إلغاء",
                ["Confirm"] = "تأكيد",
                ["Login"] = "تسجيل الدخول",
                ["Register"] = "التسجيل",
                ["Logout"] = "تسجيل الخروج",
                ["Profile"] = "الملف الشخصي",

                // Home page
                ["SearchForDestination"] = "ابحث عن وجهة أحلامك",
                ["DepartureDate"] = "تاريخ المغادرة",
                ["ReturnDate"] = "تاريخ العودة",
                ["Travelers"] = "المسافرون",
                ["FindPackages"] = "البحث عن العروض",

                // Booking
                ["BookingDetails"] = "تفاصيل الحجز",
                ["PaymentInformation"] = "معلومات الدفع",
                ["TravelerInformation"] = "معلومات المسافر",
                ["FlightInformation"] = "معلومات الرحلة",
                ["HotelInformation"] = "معلومات الفندق",
                ["CompleteBooking"] = "إتمام الحجز",

                // Order History
                ["MyBookings"] = "حجوزاتي",
                ["UpcomingTrips"] = "الرحلات القادمة",
                ["PastTrips"] = "الرحلات السابقة",
                ["BookingReference"] = "رقم مرجع الحجز",
                ["BookingDate"] = "تاريخ الحجز",
                ["TravelDates"] = "تواريخ السفر",
                ["Destination"] = "الوجهة",
                ["Status"] = "الحالة",
                ["TotalAmount"] = "المبلغ الإجمالي",

                // Chat
                ["ChatRooms"] = "غرف الدردشة",
                ["CustomerSupport"] = "دعم العملاء",
                ["SendMessage"] = "إرسال رسالة",
                ["TypeMessage"] = "اكتب رسالتك هنا...",

                // GDPR
                ["PersonalData"] = "البيانات الشخصية",
                ["DownloadData"] = "تنزيل بياناتي",
                ["DeleteAccount"] = "حذف حسابي",
                ["PrivacySettings"] = "إعدادات الخصوصية"
            };

            // Store translations
            _translations["en"] = englishTranslations;
            _translations["ar"] = arabicTranslations;
        }

        // Get current language
        public string GetCurrentLanguage()
        {
            return _currentLanguage;
        }

        // Set language
        public void SetLanguage(string languageCode)
        {
            if (_translations.ContainsKey(languageCode))
            {
                _currentLanguage = languageCode;

                // Update page direction for Arabic
                if (languageCode == "ar")
                {
                    CultureInfo.CurrentCulture = new CultureInfo("ar-SA");
                    CultureInfo.CurrentUICulture = new CultureInfo("ar-SA");
                }
                else
                {
                    CultureInfo.CurrentCulture = new CultureInfo("en-US");
                    CultureInfo.CurrentUICulture = new CultureInfo("en-US");
                }
            }
        }

        // Get translation
        public string GetTranslation(string key)
        {
            if (_translations.TryGetValue(_currentLanguage, out var translations) &&
                translations.TryGetValue(key, out var translation))
            {
                return translation;
            }

            // Fallback to English
            if (_currentLanguage != "en" &&
                _translations.TryGetValue("en", out var englishTranslations) &&
                englishTranslations.TryGetValue(key, out var englishTranslation))
            {
                return englishTranslation;
            }

            // Return the key itself if no translation found
            return key;
        }

        // Get all supported languages
        public Dictionary<string, string> GetSupportedLanguages()
        {
            return new Dictionary<string, string>
            {
                ["en"] = "English",
                ["ar"] = "العربية" // Arabic
            };
        }
    }
}