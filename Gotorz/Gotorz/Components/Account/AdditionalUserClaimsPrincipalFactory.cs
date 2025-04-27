﻿using Gotorz.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

public class AdditionalUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
	public AdditionalUserClaimsPrincipalFactory(
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole> roleManager,
		IOptions<IdentityOptions> optionsAccessor)
		: base(userManager, roleManager, optionsAccessor)
	{
	}

	protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
	{
		var identity = await base.GenerateClaimsAsync(user);
		var roles = await UserManager.GetRolesAsync(user);

		foreach (var role in roles)
		{
			identity.AddClaim(new Claim(ClaimTypes.Role, role));
		}

		return identity;
	}
}
