using JWT.Data.Helpers;
using JWT.Data.Models;
using JWT.Data.Models.Dtos;
using JWT.Helpers;
using JWT.Services.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace JWT.Services.Implementation
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ITokenService _tokenService;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly JwtSettings _jwtSettings;

		public AuthService(UserManager<ApplicationUser> userManager,
			IOptions<JwtSettings> jwtSettingsOptions,
			ITokenService tokenService,
			RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_tokenService = tokenService;
			_roleManager = roleManager;
			_jwtSettings = jwtSettingsOptions.Value;
		}


		public async Task<AuthDto> RegisterUserAsync(RegisterDto model)
		{
			if (await _userManager.FindByEmailAsync(model.Email) is not null)
				return new AuthDto() { Message = $"{model.Email} is already registered" };

			if (await _userManager.FindByNameAsync(model.UserName) is not null)
				return new AuthDto() { Message = $"{model.UserName} is already registered" };

			var user = new ApplicationUser()
			{
				Email = model.Email,
				UserName = model.UserName,
				FirstName = model.FirstName,
				LastName = model.LastName
			};

			var result = await _userManager.CreateAsync(user, model.Password);
			if (!result.Succeeded)
			{
				var message = new StringBuilder();
				foreach (var error in result.Errors)
					message.Append($"{error.Description} ");

				return new AuthDto() { Message = message.ToString() };
			}

			await _userManager.AddToRoleAsync(user, Roles.User);

			var jwtSecurityToken = await _tokenService.GenerateTokenAsync(user);
			var refreshToken = GenerateRefreshToken();
			user.RefreshTokens = [refreshToken];
			await _userManager.UpdateAsync(user);

			return new AuthDto()
			{
				Email = model.Email,
				UserName = model.UserName,
				Roles = [Roles.User],
				IsAuthenticated = true,
				Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
				TokenExpiresOn = jwtSecurityToken.ValidTo,
				RefreshToken = refreshToken.Token,
				RefreshTokenExpiration = refreshToken.ExpiresOn
			};
		}
		public async Task<AuthDto> LoginUserAsync(LoginDto model)
		{
			var authModel = new AuthDto();

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
			{
				authModel.Message = "Invalid email or password!";
				return authModel;
			}

			var jwtSecurityToken = await _tokenService.GenerateTokenAsync(user);
			var roles = await _userManager.GetRolesAsync(user);

			authModel.UserName = user.UserName!;
			authModel.Email = user.Email!;
			authModel.Roles = roles.ToList();
			authModel.IsAuthenticated = true;
			authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
			authModel.TokenExpiresOn = jwtSecurityToken.ValidTo;

			if (user.RefreshTokens.Any(t => t.IsActive))
			{
				var activeRefreshToken = user.RefreshTokens.Single(t => t.IsActive)!;

				authModel.RefreshToken = activeRefreshToken.Token;
				authModel.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
			}

			else
			{
				var refreshToken = GenerateRefreshToken();

				authModel.RefreshToken = refreshToken.Token;
				authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;

				user.RefreshTokens.Add(refreshToken);
				await _userManager.UpdateAsync(user);
			}

			return authModel;
		}
		public async Task<string> AddRoleAsync(AddRoleDto model)
		{
			var user = await _userManager.FindByIdAsync(model.UserId);
			if (user is null || !await _roleManager.RoleExistsAsync(model.RoleName))
				return "Invalid UserId or RoleName";

			if (await _userManager.IsInRoleAsync(user, model.RoleName))
				return "This user is already assigned to this role";

			var result = await _userManager.AddToRoleAsync(user, model.RoleName);
			return result.Succeeded
				? string.Empty
				: "Something went wrong!";
		}
		public async Task<AuthDto> RefreshTokenAsync(string refreshToken)
		{

			// Checking if there is a user related to this refresh token
			var user = await _userManager.Users.SingleOrDefaultAsync(user => user.RefreshTokens.Any(t => t.Token == refreshToken));


			if (user is null) // this means this refresh token doesn't belong to any of the users
				return new AuthDto() { Message = "Invalid token" };


			// Getting here means the refresh token sent belongs to user

			var existingRefreshToken = user.RefreshTokens.Single(t => t.Token == refreshToken);

			if (!existingRefreshToken.IsActive)
				return new AuthDto() { Message = "Inactive token" }; // Incase the refresh token is inactive

			//  Getting here means the refresh token sent belongs to user & is active 

			// Revoke this refresh token
			existingRefreshToken.RevokedOn = DateTime.UtcNow;

			// Creates new refresh token
			var newRefreshToken = GenerateRefreshToken();

			// adds this new refresh to user RefreshTokens list
			user.RefreshTokens.Add(newRefreshToken);

			// updating user
			await _userManager.UpdateAsync(user);


			var userRoles = await _userManager.GetRolesAsync(user);
			var jwtToken = await _tokenService.GenerateTokenAsync(user);

			return new AuthDto()
			{
				Email = user.Email ?? string.Empty,
				UserName = user.UserName ?? string.Empty,
				IsAuthenticated = true,
				Roles = userRoles.ToList(),

				Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
				RefreshToken = newRefreshToken.Token,
				RefreshTokenExpiration = newRefreshToken.ExpiresOn
			};


		}
		public async Task<bool> RevokeTokenAsync(string refreshToken)
		{
			var user = await _userManager.Users.SingleOrDefaultAsync(user => user.RefreshTokens.Any(t => t.Token == refreshToken));
			if (user is null)
				return false;

			var existingRefreshToken = user.RefreshTokens.Single(t => t.Token == refreshToken);
			if (!existingRefreshToken.IsActive)
				return false;

			existingRefreshToken.RevokedOn = DateTime.UtcNow;
			var result = await _userManager.UpdateAsync(user);

			return result.Succeeded;
		}
		private static RefreshToken GenerateRefreshToken()
			=> new()
			{
				CreatedOn = DateTime.UtcNow,
				ExpiresOn = DateTime.UtcNow.AddDays(10),
				Token = Guid.NewGuid().ToString()
			};
	}
}
