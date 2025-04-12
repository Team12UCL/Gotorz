using Gotorz.Data;
using Gotorz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gotorz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManagementService _roleManagementService;
        private readonly ActivityLogService _activityLogService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManagementService roleManagementService,
            ActivityLogService activityLogService,
            ILogger<AdminController> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManagementService = roleManagementService;
            _activityLogService = activityLogService;
            _logger = logger;
        }

        // GET: api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .Skip(skip)
                .Take(take)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.PhoneNumber,
                    u.EmailConfirmed,
                    u.LockoutEnabled,
                    u.LockoutEnd
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/admin/users/{userId}/roles
        [HttpGet("users/{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var roles = await _roleManagementService.GetUserRolesAsync(userId);
            return Ok(roles);
        }

        // POST: api/admin/users/{userId}/roles
        [HttpPost("users/{userId}/roles")]
        public async Task<IActionResult> AddUserToRole(string userId, [FromBody] string roleName)
        {
            var result = await _roleManagementService.AddUserToRoleAsync(userId, roleName);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByIdAsync(userId);
                await _activityLogService.LogActivityAsync(
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    "AdminAction",
                    $"Added user {user.UserName} to role {roleName}",
                    HttpContext.Connection.RemoteIpAddress.ToString());

                return Ok();
            }

            return BadRequest(result.Errors);
        }

        // DELETE: api/admin/users/{userId}/roles/{roleName}
        [HttpDelete("users/{userId}/roles/{roleName}")]
        public async Task<IActionResult> RemoveUserFromRole(string userId, string roleName)
        {
            var result = await _roleManagementService.RemoveUserFromRoleAsync(userId, roleName);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByIdAsync(userId);
                await _activityLogService.LogActivityAsync(
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    "AdminAction",
                    $"Removed user {user.UserName} from role {roleName}",
                    HttpContext.Connection.RemoteIpAddress.ToString());

                return Ok();
            }

            return BadRequest(result.Errors);
        }

        // GET: api/admin/roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleManagementService.GetAllRolesAsync();
            return Ok(roles);
        }

        // POST: api/admin/roles
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            var result = await _roleManagementService.CreateRoleAsync(roleName);

            if (result.Succeeded)
            {
                await _activityLogService.LogActivityAsync(
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    "AdminAction",
                    $"Created new role {roleName}",
                    HttpContext.Connection.RemoteIpAddress.ToString());

                return Ok();
            }

            return BadRequest(result.Errors);
        }

        // DELETE: api/admin/roles/{roleName}
        [HttpDelete("roles/{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var result = await _roleManagementService.DeleteRoleAsync(roleName);

            if (result.Succeeded)
            {
                await _activityLogService.LogActivityAsync(
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    "AdminAction",
                    $"Deleted role {roleName}",
                    HttpContext.Connection.RemoteIpAddress.ToString());

                return Ok();
            }

            return BadRequest(result.Errors);
        }

        // GET: api/admin/bookings
        [HttpGet("bookings")]
        public async Task<IActionResult> GetAllBookings([FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            var bookings = await _dbContext.Bookings
                .Include(b => b.TravelPackage)
                .OrderByDescending(b => b.BookingDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return Ok(bookings);
        }

        // GET: api/admin/activity-logs
        [HttpGet("activity-logs")]
        public async Task<IActionResult> GetActivityLogs(
            [FromQuery] string activityType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100)
        {
            var logs = await _activityLogService.GetAllActivityLogsAsync(
                activityType, startDate, endDate, skip, take);

            return Ok(logs);
        }

        // GET: api/admin/activity-logs/export
        [HttpGet("activity-logs/export")]
        public async Task<IActionResult> ExportActivityLogs(
            [FromQuery] string userId = null,
            [FromQuery] string activityType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var csvData = await _activityLogService.ExportActivityLogsAsCsvAsync(
                userId, activityType, startDate, endDate);

            if (csvData == null)
            {
                return StatusCode(500, "Failed to export activity logs");
            }

            return File(csvData, "text/csv", "activity_logs.csv");
        }

        // POST: api/admin/reset-password/{userId}
        [HttpPost("reset-password/{userId}")]
        public async Task<IActionResult> ResetUserPassword(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // In a real application, you would email this token to the user
            // For this implementation, we'll just log it
            _logger.LogInformation($"Password reset token for user {user.UserName}: {token}");

            await _activityLogService.LogActivityAsync(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                "AdminAction",
                $"Generated password reset token for user {user.UserName}",
                HttpContext.Connection.RemoteIpAddress.ToString());

            return Ok(new { Message = "Password reset token generated successfully" });
        }

        // GET: api/admin/dashboard-stats
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalBookings = await _dbContext.Bookings.CountAsync();
            var totalSalesAmount = await _dbContext.Bookings.SumAsync(b => b.TotalPrice);
            var recentBookings = await _dbContext.Bookings
                .OrderByDescending(b => b.BookingDate)
                .Take(5)
                .ToListAsync();

            // Get active user count (users with activity in last 30 days)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var activeUsersCount = await _dbContext.ActivityLogs
                .Where(a => a.Timestamp >= thirtyDaysAgo)
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();

            return Ok(new
            {
                TotalUsers = totalUsers,
                TotalBookings = totalBookings,
                TotalSales = totalSalesAmount,
                ActiveUsers = activeUsersCount,
                RecentBookings = recentBookings
            });
        }
    }
}