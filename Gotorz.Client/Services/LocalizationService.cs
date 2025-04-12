using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Gotorz.Client.Services
{
    public class LocalizationService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _translations = new();
        private CultureInfo _currentCulture = CultureInfo.GetCultureInfo("en-US");
        private readonly string[] _supportedLanguages = new[] { "en-US", "fr-FR", "de-DE", "es-ES", "it-IT" };

        public LocalizationService()
        {
            // Initialize with sample translations
            InitializeTranslations();
        }

        private void InitializeTranslations()
        {
            // English translations (default)
            var enTranslations = new Dictionary<string, string>
            {
                // Common
                { "app.name", "GodTur" },
                { "app.tagline", "Discover the world with us" },
                
                // Navigation
                { "nav.home", "Home" },
                { "nav.packages", "Travel Packages" },
                { "nav.flights", "Flights" },
                { "nav.hotels", "Hotels" },
                { "nav.bookings", "My Bookings" },
                { "nav.account", "My Account" },
                { "nav.support", "Support" },
                
                // Search
                { "search.destination", "Where do you want to go?" },
                { "search.dates", "When do you want to travel?" },
                { "search.travelers", "How many travelers?" },
                { "search.button", "Search" },
                
                // Package details
                { "package.from", "from" },
                { "package.night", "night" },
                { "package.nights", "nights" },
                { "package.includes", "What's included" },
                { "package.book", "Book Now" },
                { "package.customize", "Customize" },
                
                // Booking
                { "booking.details", "Booking Details" },
                { "booking.travelers", "Traveler Information" },
                { "booking.payment", "Payment" },
                { "booking.confirmation", "Confirmation" },
                { "booking.complete", "Complete Booking" },
                
                // Account
                { "account.profile", "Profile" },
                { "account.password", "Password" },
                { "account.payment", "Payment Methods" },
                { "account.preferences", "Preferences" },
                { "account.notifications", "Notifications" },
                
                // Footer
                { "footer.about", "About Us" },
                { "footer.terms", "Terms & Conditions" },
                { "footer.privacy", "Privacy Policy" },
                { "footer.contact", "Contact Us" },
                { "footer.copyright", "© 2025 GodTur. All rights reserved." }
            };

            // French translations
            var frTranslations = new Dictionary<string, string>
            {
                // Common
                { "app.name", "GodTur" },
                { "app.tagline", "Découvrez le monde avec nous" },
                
                // Navigation
                { "nav.home", "Accueil" },
                { "nav.packages", "Forfaits Voyage" },
                { "nav.flights", "Vols" },
                { "nav.hotels", "Hôtels" },
                { "nav.bookings", "Mes Réservations" },
                { "nav.account", "Mon Compte" },
                { "nav.support", "Assistance" },
                
                // Search
                { "search.destination", "Où voulez-vous aller ?" },
                { "search.dates", "Quand voulez-vous voyager ?" },
                { "search.travelers", "Combien de voyageurs ?" },
                { "search.button", "Rechercher" },
                
                // Package details
                { "package.from", "à partir de" },
                { "package.night", "nuit" },
                { "package.nights", "nuits" },
                { "package.includes", "Ce qui est inclus" },
                { "package.book", "Réserver Maintenant" },
                { "package.customize", "Personnaliser" },
                
                // Booking
                { "booking.details", "Détails de la Réservation" },
                { "booking.travelers", "Informations sur les Voyageurs" },
                { "booking.payment", "Paiement" },
                { "booking.confirmation", "Confirmation" },
                { "booking.complete", "Finaliser la Réservation" },
                
                // Account
                { "account.profile", "Profil" },
                { "account.password", "Mot de Passe" },
                { "account.payment", "Méthodes de Paiement" },
                { "account.preferences", "Préférences" },
                { "account.notifications", "Notifications" },
                
                // Footer
                { "footer.about", "À Propos de Nous" },
                { "footer.terms", "Conditions Générales" },
                { "footer.privacy", "Politique de Confidentialité" },
                { "footer.contact", "Nous Contacter" },
                { "footer.copyright", "© 2025 GodTur. Tous droits réservés." }
            };

            // German translations
            var deTranslations = new Dictionary<string, string>
            {
                // Common
                { "app.name", "GodTur" },
                { "app.tagline", "Entdecken Sie die Welt mit uns" },
                
                // Navigation
                { "nav.home", "Startseite" },
                { "nav.packages", "Reisepakete" },
                { "nav.flights", "Flüge" },
                { "nav.hotels", "Hotels" },
                { "nav.bookings", "Meine Buchungen" },
                { "nav.account", "Mein Konto" },
                { "nav.support", "Support" },
                
                // Add more German translations as needed
            };

            // Spanish translations (partial)
            var esTranslations = new Dictionary<string, string>
            {
                // Common
                { "app.name", "GodTur" },
                { "app.tagline", "Descubre el mundo con nosotros" },
                
                // Navigation
                { "nav.home", "Inicio" },
                { "nav.packages", "Paquetes de Viaje" },
                { "nav.flights", "Vuelos" },
                { "nav.hotels", "Hoteles" },
                { "nav.bookings", "Mis Reservas" },
                { "nav.account", "Mi Cuenta" },
                { "nav.support", "Soporte" },
                
                // Add more Spanish translations as needed
            };

            // Italian translations (partial)
            var itTranslations = new Dictionary<string, string>
            {
                // Common
                { "app.name", "GodTur" },
                { "app.tagline", "Scopri il mondo con noi" },
                
                // Navigation
                { "nav.home", "Home" },
                { "nav.packages", "Pacchetti Viaggio" },
                { "nav.flights", "Voli" },
                { "nav.hotels", "Hotel" },
                { "nav.bookings", "Le Mie Prenotazioni" },
                { "nav.account", "Il Mio Account" },
                { "nav.support", "Supporto" },
                
                // Add more Italian translations as needed
            };

            // Add all translations to the main dictionary
            _translations.Add("en-US", enTranslations);
            _translations.Add("fr-FR", frTranslations);
            _translations.Add("de-DE", deTranslations);
            _translations.Add("es-ES", esTranslations);
            _translations.Add("it-IT", itTranslations);
        }

        // Get a translated string by key
        public string GetString(string key)
        {
            var cultureName = _currentCulture.Name;

            // Try to get the string in the current culture
            if (_translations.ContainsKey(cultureName) &&
                _translations[cultureName].ContainsKey(key))
            {
                return _translations[cultureName][key];
            }

            // Fall back to English if the key doesn't exist in the current culture
            if (_translations["en-US"].ContainsKey(key))
            {
                return _translations["en-US"][key];
            }

            // If all else fails, return the key itself
            return key;
        }

        // Set the current culture
        public void SetCulture(string cultureName)
        {
            if (_supportedLanguages.Contains(cultureName))
            {
                _currentCulture = CultureInfo.GetCultureInfo(cultureName);
                // In a real app, you might want to persist this choice
                // and also update the thread culture
                CultureInfo.CurrentCulture = _currentCulture;
                CultureInfo.CurrentUICulture = _currentCulture;
            }
        }

        // Get the current culture
        public CultureInfo GetCurrentCulture()
        {
            return _currentCulture;
        }

        // Get all supported cultures
        public IEnumerable<CultureInfo> GetSupportedCultures()
        {
            return _supportedLanguages.Select(CultureInfo.GetCultureInfo);
        }

        // Get a name for the language in its native form
        public string GetLanguageNativeName(string cultureName)
        {
            return CultureInfo.GetCultureInfo(cultureName).NativeName;
        }

        // Format currency according to the current culture
        public string FormatCurrency(decimal amount, string currencyCode = "USD")
        {
            return amount.ToString("C", new CultureInfo(_currentCulture.Name) { NumberFormat = { CurrencySymbol = GetCurrencySymbol(currencyCode) } });
        }

        // Format date according to the current culture
        public string FormatDate(DateTime date, string format = "d")
        {
            return date.ToString(format, _currentCulture);
        }

        // Get currency symbol based on currency code
        private string GetCurrencySymbol(string currencyCode)
        {
            switch (currencyCode)
            {
                case "USD": return "$";
                case "EUR": return "€";
                case "GBP": return "£";
                case "JPY": return "¥";
                default: return currencyCode;
            }
        }
    }
}