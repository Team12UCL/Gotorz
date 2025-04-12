using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Shared.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Gotorz.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendBookingConfirmationEmailAsync(Booking booking, string userEmail, string userName)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(
                    emailSettings["SenderName"],
                    emailSettings["SenderEmail"]));

                message.To.Add(new MailboxAddress(userName, userEmail));
                message.Subject = $"Your GodTur Booking Confirmation - {booking.BookingReference}";

                var builder = new BodyBuilder();

                // HTML body with booking details
                builder.HtmlBody = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .header {{ background-color: #003366; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .footer {{ background-color: #f4f4f4; padding: 10px; text-align: center; font-size: 12px; }}
                        .booking-details {{ background-color: #f9f9f9; border: 1px solid #ddd; padding: 15px; margin: 20px 0; }}
                        .important {{ color: #003366; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>GodTur Travel</h1>
                    </div>
                    <div class='content'>
                        <h2>Booking Confirmation</h2>
                        <p>Dear {userName},</p>
                        <p>Thank you for choosing GodTur for your travel needs. Your booking has been confirmed!</p>
                        
                        <div class='booking-details'>
                            <h3>Booking Details</h3>
                            <p><span class='important'>Booking Reference:</span> {booking.BookingReference}</p>
                            <p><span class='important'>Package Name:</span> {booking.TravelPackage?.Name}</p>
                            <p><span class='important'>Destination:</span> {booking.TravelPackage?.Destination}</p>
                            <p><span class='important'>Departure Date:</span> {booking.DepartureDate:dddd, MMMM d, yyyy}</p>
                            <p><span class='important'>Return Date:</span> {booking.ReturnDate:dddd, MMMM d, yyyy}</p>
                            <p><span class='important'>Number of Travelers:</span> {booking.NumberOfTravelers}</p>
                            <p><span class='important'>Total Price:</span> {booking.TotalPrice:C} {booking.Currency}</p>
                        </div>
                        
                        <p>Your e-tickets and detailed travel itinerary are attached to this email.</p>
                        
                        <p>If you have any questions or need assistance, please don't hesitate to contact our customer service team at <a href='mailto:support@godtur.com'>support@godtur.com</a> or call us at +1-800-GODTUR.</p>
                        
                        <p>We wish you a pleasant journey!</p>
                        
                        <p>Warm regards,<br>The GodTur Team</p>
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} GodTur Travel. All rights reserved.</p>
                        <p>This email was sent to {userEmail}. If you didn't make this booking, please contact us immediately.</p>
                    </div>
                </body>
                </html>";

                // Generate PDF attachments
                if (booking.TravelPackage != null)
                {
                    // E-ticket attachment
                    var eTicketPdf = GenerateETicketPdf(booking);
                    if (eTicketPdf != null)
                    {
                        builder.Attachments.Add("e-ticket.pdf", eTicketPdf, ContentType.Parse("application/pdf"));
                    }

                    // Travel itinerary attachment
                    var itineraryPdf = GenerateTravelItineraryPdf(booking);
                    if (itineraryPdf != null)
                    {
                        builder.Attachments.Add("travel-itinerary.pdf", itineraryPdf, ContentType.Parse("application/pdf"));
                    }

                    // Hotel voucher
                    var hotelVoucherPdf = GenerateHotelVoucherPdf(booking);
                    if (hotelVoucherPdf != null)
                    {
                        builder.Attachments.Add("hotel-voucher.pdf", hotelVoucherPdf, ContentType.Parse("application/pdf"));
                    }

                    // Travel guide for the destination
                    var travelGuidePdf = GetTravelGuidePdf(booking.TravelPackage.Destination);
                    if (travelGuidePdf != null)
                    {
                        builder.Attachments.Add($"{booking.TravelPackage.Destination}-travel-guide.pdf", travelGuidePdf, ContentType.Parse("application/pdf"));
                    }
                }

                message.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // Connect to SMTP server
                    await client.ConnectAsync(
                        emailSettings["SmtpServer"],
                        int.Parse(emailSettings["SmtpPort"]),
                        SecureSocketOptions.StartTls);

                    // Authenticate
                    await client.AuthenticateAsync(
                        emailSettings["SmtpUsername"],
                        emailSettings["SmtpPassword"]);

                    // Send the message
                    await client.SendAsync(message);

                    // Disconnect
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Booking confirmation email sent to {userEmail} for booking {booking.BookingReference}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending booking confirmation email for booking {booking.BookingReference}");
                return false;
            }
        }

        // In a real application, these methods would generate actual PDF documents
        // For now, they return placeholder byte arrays
        private byte[] GenerateETicketPdf(Booking booking)
        {
            // For demo purposes, return a dummy PDF byte array
            // In a real app, use a PDF generation library like iTextSharp or PdfSharp
            return new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header bytes
        }

        private byte[] GenerateTravelItineraryPdf(Booking booking)
        {
            // For demo purposes, return a dummy PDF byte array
            return new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header bytes
        }

        private byte[] GenerateHotelVoucherPdf(Booking booking)
        {
            // For demo purposes, return a dummy PDF byte array
            return new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header bytes
        }

        private byte[] GetTravelGuidePdf(string destination)
        {
            // In a real app, this would fetch a prepared travel guide PDF for the specific destination
            // For demo purposes, return a dummy PDF byte array
            return new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header bytes
        }

        public async Task<bool> SendTravelGuideEmailAsync(string userEmail, string userName, string destination)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(
                    emailSettings["SenderName"],
                    emailSettings["SenderEmail"]));

                message.To.Add(new MailboxAddress(userName, userEmail));
                message.Subject = $"Your GodTur Travel Guide for {destination}";

                var builder = new BodyBuilder();

                // HTML body with travel guide information
                builder.HtmlBody = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .header {{ background-color: #003366; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .footer {{ background-color: #f4f4f4; padding: 10px; text-align: center; font-size: 12px; }}
                        .guide-section {{ margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>GodTur Travel</h1>
                    </div>
                    <div class='content'>
                        <h2>Your Travel Guide for {destination}</h2>
                        <p>Dear {userName},</p>
                        <p>We're excited for your upcoming trip to {destination}! To help you make the most of your journey, we've prepared this comprehensive travel guide.</p>
                        
                        <div class='guide-section'>
                            <h3>What to Know Before You Go</h3>
                            <p>Find essential information about local customs, currency, transportation options, and safety tips in the attached guide.</p>
                        </div>
                        
                        <div class='guide-section'>
                            <h3>Must-See Attractions</h3>
                            <p>Discover the top attractions, hidden gems, and local favorites in {destination}.</p>
                        </div>
                        
                        <div class='guide-section'>
                            <h3>Transportation Maps</h3>
                            <p>Navigate {destination} like a local with our detailed transportation maps.</p>
                        </div>
                        
                        <p>We hope you find this guide helpful. If you have any questions or need further assistance, please contact our customer service team.</p>
                        
                        <p>Warm regards,<br>The GodTur Team</p>
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} GodTur Travel. All rights reserved.</p>
                    </div>
                </body>
                </html>";

                // Attach the travel guide PDF
                var travelGuidePdf = GetTravelGuidePdf(destination);
                if (travelGuidePdf != null)
                {
                    builder.Attachments.Add($"{destination}-travel-guide.pdf", travelGuidePdf, ContentType.Parse("application/pdf"));
                }

                // Attach transportation maps
                var transportationMapPdf = GetTransportationMapPdf(destination);
                if (transportationMapPdf != null)
                {
                    builder.Attachments.Add($"{destination}-transportation-map.pdf", transportationMapPdf, ContentType.Parse("application/pdf"));
                }

                message.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // Connect to SMTP server
                    await client.ConnectAsync(
                        emailSettings["SmtpServer"],
                        int.Parse(emailSettings["SmtpPort"]),
                        SecureSocketOptions.StartTls);

                    // Authenticate
                    await client.AuthenticateAsync(
                        emailSettings["SmtpUsername"],
                        emailSettings["SmtpPassword"]);

                    // Send the message
                    await client.SendAsync(message);

                    // Disconnect
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Travel guide email sent to {userEmail} for {destination}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending travel guide email for {destination} to {userEmail}");
                return false;
            }
        }

        private byte[] GetTransportationMapPdf(string destination)
        {
            // In a real app, this would fetch transportation maps for the specific destination
            // For demo purposes, return a dummy PDF byte array
            return new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header bytes
        }
    }
}