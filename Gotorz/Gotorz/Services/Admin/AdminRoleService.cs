using Gotorz.Services;
using Microsoft.AspNetCore.Identity;

namespace Gotorz.Services.Admin
{
	public class AdminRoleService
	{
		private readonly RoleManagementService _roleManagementService;
		private readonly ActivityLogService _activityLogService;

		public AdminRoleService(RoleManagementService roleManagementService, ActivityLogService activityLogService)
		{
			_roleManagementService = roleManagementService;
			_activityLogService = activityLogService;
		}

		public Task<List<string>> GetAllRolesAsync()
		{
			return _roleManagementService.GetAllRolesAsync();
		}

		public Task<IdentityResult> CreateRoleAsync(string roleName)
		{
			return _roleManagementService.CreateRoleAsync(roleName);
		}

		public Task<IdentityResult> DeleteRoleAsync(string roleName)
		{
			return _roleManagementService.DeleteRoleAsync(roleName);
		}

		public Task<List<string>> GetUserRolesAsync(string userId)
		{
			return _roleManagementService.GetUserRolesAsync(userId);
		}

		public Task<IdentityResult> AddUserToRoleAsync(string userId, string roleName)
		{
			return _roleManagementService.AddUserToRoleAsync(userId, roleName);
		}

		public Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string roleName)
		{
			return _roleManagementService.RemoveUserFromRoleAsync(userId, roleName);
		}
	}
}
