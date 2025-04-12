using Gotorz.Data;
using Gotorz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Gotorz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SalesAgent")]
    public class SalesAgentController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ActivityLogService _activityLogService;
        private readonly EmailService _emailService;
        private readonly TravelGuideService _travelGuideService;
        private readonly ILogger<SalesAgentController> _logger;

        public SalesAgentController(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            ActivityLogService activityLogService,
            EmailService emailService,
            TravelGuideService travelGuideService,
            ILogger<SalesAgentController> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _activityLogService = activityLogService;
            _emailService = emailService;
            _travelGuideService = travelGuideService;
            _logger = logger;
        }

        // GET: api/salesagent/customers
        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers([FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            // Get all users with the Customer role
            var customerRoleId = (await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Customer"))?.Id;
            if (customerRoleId == null)
            {
                return Ok(new { });
            }

            var customerUserIds = await _dbContext.UserRoles
                .Where(ur => ur.RoleId == customerRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var customers = await _userManager.Users
                .Where(u => customerUserIds.Contains(u.Id))
                .OrderBy(u => u.UserName)
                .Skip(skip)
                .Take(take)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.PhoneNumber,
                    u.FirstName,
                    u.LastName
                })
                .ToListAsync();

            return Ok(customers);
        }

        // GET: api/salesagent/customers/{customerId}/bookings
        [HttpGet("customers/{customerId}/bookings")]
        public async Task<IActionResult> GetCustomerBookings(string customerId)
        {
            var bookings = await _dbContext.Bookings
                .Where(b => b.UserId == customerId)
                .Include(b => b.TravelPackage)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return Ok(bookings);
        }

        // POST: api/salesagent/customers/{customerId}/send-travel-guide
        [HttpPost("customers/{customerId}/send-travel-guide")]
        public async Task<IActionResult> SendTravelGuide(string customerId, [FromBody] string destination)
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                return NotFound($"Customer with ID {customerId} not found.");
            }

            var success = await _travelGuideService.SendTravelGuideToUserAsync(
                destination,
                customer.Email,
                $"{customer.FirstName} {customer.LastName}");

            if (success)
            {
                await _activityLogService.LogActivityAsync(
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    "SalesAgentAction",
                    $"Sent travel guide for {destination} to customer {customer.UserName}",
                    HttpContext.Connection.RemoteIpAddress.ToString());

                return Ok(new { Message = $"Travel guide for {destination} has been sent to {customer.Email}" });
            }

            return StatusCode(500, "Failed to send travel guide");
        }

        // POST: api/salesagent/customers/{customerId}/send-booking-confirmation/{bookingId}
        [HttpPost("customers/{customerId}/send-booking-confirmation/{bookingId}")]
        public async Task<IActionResult> ResendBookingConfirmation(string customerId, Guid bookingId)
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null)
            {
                return NotFound($"Customer with ID {customerId} not found.");
            }

            var booking = await _dbContext.Bookings
                .Include(b => b.TravelPackage)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == customerId);

            if (booking == null)
            {
                return NotFound($"Booking with ID {bookingId} not found for this customer.");
            }

            var success = await _emailService.SendBookingConfirmationEmailAsync(
                booking,
                customer.Email,
                $"{customer.FirstName} {customer.LastName}");

            if (success)
            {
                await _activityLogService.LogActivityAsync(
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    "SalesAgentAction",
                    $"Resent booking confirmation {booking.BookingReference} to customer {customer.UserName}",
                    HttpContext.Connection.RemoteIpAddress.ToString());

                return Ok(new { Message = $"Booking confirmation has been sent to {customer.Email}" });
            }

            return StatusCode(500, "Failed to send booking confirmation");
        }

        // GET: api/salesagent/dashboard-stats
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var agentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Get assigned customers (in a real application, this would be based on customer assignments to agents)
            var customerCount = await _userManager.Users.CountAsync();

            // Get bookings assisted by this agent
            // For demo purposes, we'll just count all bookings
            var assistedBookings = await _dbContext.Bookings.CountAsync();

            // Get monthly sales data
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
            var monthlyBookings = await _dbContext.Bookings
                .Where(b => b.BookingDate >= firstDayOfMonth)
                .CountAsync();

            var monthlySalesAmount = await _dbContext.Bookings
                .Where(b => b.BookingDate >= firstDayOfMonth)
                .SumAsync(b => b.TotalPrice);

            return Ok(new
            {
                CustomersCount = customerCount,
                AssistedBookings = assistedBookings,
                MonthlyBookings = monthlyBookings,
                MonthlySales = monthlySalesAmount
            });
        }

        // GET: api/salesagent/support-tickets
        [HttpGet("support-tickets")]
        public async Task<IActionResult> GetSupportTickets(
            [FromQuery] string status = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20)
        {
            var query = _dbContext.SupportTickets.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return Ok(tickets);
        }

        // PUT: api/salesagent/support-tickets/{ticketId}
        [HttpPut("support-tickets/{ticketId}")]
        public async Task<IActionResult> UpdateSupportTicket(Guid ticketId, [FromBody] SupportTicket updatedTicket)
        {
            var ticket = await _dbContext.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
            {
                return NotFound($"Support ticket with ID {ticketId} not found.");
            }

            // Update only allowed fields
            ticket.Status = updatedTicket.Status;
            ticket.AgentNotes = updatedTicket.AgentNotes;
            ticket.AssignedAgentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            ticket.LastUpdated = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            await _activityLogService.LogActivityAsync(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                "SalesAgentAction",
                $"Updated support ticket {ticketId} status to {ticket.Status}",
                HttpContext.Connection.RemoteIpAddress.ToString());

            return Ok(ticket);
        }
    }
}