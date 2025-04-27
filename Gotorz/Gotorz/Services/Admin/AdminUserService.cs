using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Gotorz.Data;
using Shared.Models.Admin;

namespace Gotorz.Services.Admin
{
	public class AdminUserService
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public AdminUserService(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<List<AdminUserDto>> GetUsersAsync(int skip = 0, int take = 100)
		{
			return await _userManager.Users
				.OrderBy(u => u.UserName)
				.Skip(skip)
				.Take(take)
				.Select(u => new AdminUserDto
				{
					Id = u.Id,
					UserName = u.UserName,
					Email = u.Email,
					PhoneNumber = u.PhoneNumber,
					EmailConfirmed = u.EmailConfirmed,
					LockoutEnabled = u.LockoutEnabled,
					LockoutEnd = u.LockoutEnd
				})
				.ToListAsync();
		}

		public async Task<IdentityResult> CreateUserAsync(string email, string password, string role = "User")
		{
			var user = new ApplicationUser
			{
				UserName = email,
				Email = email,
				EmailConfirmed = true // You can change this depending on your logic
			};

			var createResult = await _userManager.CreateAsync(user, password);

			if (createResult.Succeeded && !string.IsNullOrEmpty(role))
			{
				await _userManager.AddToRoleAsync(user, role);
			}

			return createResult;
		}

		public async Task<IdentityResult> UpdateUserAsync(AdminUserDto updatedUser)
		{
			var user = await _userManager.FindByIdAsync(updatedUser.Id);
			if (user == null)
			{
				return IdentityResult.Failed(new IdentityError { Description = $"User with ID {updatedUser.Id} not found." });
			}

			user.UserName = updatedUser.UserName;
			user.Email = updatedUser.Email;
			user.PhoneNumber = updatedUser.PhoneNumber;
			user.LockoutEnabled = updatedUser.LockoutEnabled;
			user.LockoutEnd = updatedUser.LockoutEnd;

			return await _userManager.UpdateAsync(user);
		}

		public async Task<IdentityResult> DeleteUserAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return IdentityResult.Failed(new IdentityError { Description = $"User with ID {userId} not found." });
			}

			return await _userManager.DeleteAsync(user);
		}
	}
}
