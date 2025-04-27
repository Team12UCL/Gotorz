using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gotorz.Data;

namespace Gotorz.Services
{
	public class RoleManagementService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ILogger<RoleManagementService> _logger;

		public RoleManagementService(
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			ILogger<RoleManagementService> logger)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_logger = logger;
		}

		public async Task<List<string>> GetAllRolesAsync()
		{
			return await _roleManager.Roles.Select(r => r.Name).ToListAsync();
		}

		public async Task<bool> RoleExistsAsync(string roleName)
		{
			return await _roleManager.RoleExistsAsync(roleName);
		}

		public async Task<IdentityResult> CreateRoleAsync(string roleName)
		{
			if (await RoleExistsAsync(roleName))
			{
				_logger.LogWarning($"Role {roleName} already exists.");
				return IdentityResult.Failed(new IdentityError { Description = $"Role {roleName} already exists." });
			}

			return await _roleManager.CreateAsync(new IdentityRole(roleName));
		}

		public async Task<IdentityResult> DeleteRoleAsync(string roleName)
		{
			var role = await _roleManager.FindByNameAsync(roleName);
			if (role == null)
			{
				_logger.LogWarning($"Role {roleName} not found.");
				return IdentityResult.Failed(new IdentityError { Description = $"Role {roleName} not found." });
			}

			// Check if any users are in this role
			var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
			if (usersInRole.Any())
			{
				_logger.LogWarning($"Cannot delete role {roleName} because it has {usersInRole.Count} users.");
				return IdentityResult.Failed(new IdentityError
				{
					Description = $"Cannot delete role {roleName} because it has users assigned to it. Remove all users from this role first."
				});
			}

			return await _roleManager.DeleteAsync(role);
		}

		public async Task<List<ApplicationUser>> GetUsersInRoleAsync(string roleName)
		{
			if (!await RoleExistsAsync(roleName))
			{
				_logger.LogWarning($"Role {roleName} not found.");
				return new List<ApplicationUser>();
			}

			return (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
		}

		public async Task<List<string>> GetUserRolesAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				_logger.LogWarning($"User with ID {userId} not found.");
				return new List<string>();
			}

			return (await _userManager.GetRolesAsync(user)).ToList();
		}

		public async Task<IdentityResult> AddUserToRoleAsync(string userId, string roleName)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				_logger.LogWarning($"User with ID {userId} not found.");
				return IdentityResult.Failed(new IdentityError { Description = $"User with ID {userId} not found." });
			}

			if (!await RoleExistsAsync(roleName))
			{
				_logger.LogWarning($"Role {roleName} not found.");
				return IdentityResult.Failed(new IdentityError { Description = $"Role {roleName} not found." });
			}

			if (await _userManager.IsInRoleAsync(user, roleName))
			{
				_logger.LogWarning($"User {user.UserName} is already in role {roleName}.");
				return IdentityResult.Failed(new IdentityError { Description = $"User {user.UserName} is already in role {roleName}." });
			}

			return await _userManager.AddToRoleAsync(user, roleName);
		}

		public async Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string roleName)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				_logger.LogWarning($"User with ID {userId} not found.");
				return IdentityResult.Failed(new IdentityError { Description = $"User with ID {userId} not found." });
			}

			if (!await RoleExistsAsync(roleName))
			{
				_logger.LogWarning($"Role {roleName} not found.");
				return IdentityResult.Failed(new IdentityError { Description = $"Role {roleName} not found." });
			}

			if (!await _userManager.IsInRoleAsync(user, roleName))
			{
				_logger.LogWarning($"User {user.UserName} is not in role {roleName}.");
				return IdentityResult.Failed(new IdentityError { Description = $"User {user.UserName} is not in role {roleName}." });
			}

			return await _userManager.RemoveFromRoleAsync(user, roleName);
		}

		// Method to ensure default roles exist
		public async Task EnsureDefaultRolesAsync()
		{
			string[] defaultRoles = { "Admin", "SalesAgent", "Customer" };

			foreach (var roleName in defaultRoles)
			{
				if (!await RoleExistsAsync(roleName))
				{
					await CreateRoleAsync(roleName);
					_logger.LogInformation($"Created default role: {roleName}");
				}
			}
		}
	}
}