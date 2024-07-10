using JWT.Data.Models;
using JWT.Helpers;
using JWT.Services.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWT.Services.Implementation
{
	public class TokenService : ITokenService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly JwtSettings _jwtSettings;

		public TokenService(UserManager<ApplicationUser> userManager,
			IOptions<JwtSettings> jwtSettingsOptions)
		{
			_userManager = userManager;
			_jwtSettings = jwtSettingsOptions.Value;
		}
		public async Task<JwtSecurityToken> GenerateTokenAsync(ApplicationUser applicationUser)
		{
			var claims = new List<Claim>()
			{
				// Private Claims
				new (ClaimTypes.Name, applicationUser.UserName ?? string.Empty),
				new (ClaimTypes.NameIdentifier, applicationUser.Id),
				new (ClaimTypes.Email, applicationUser.Email ?? string.Empty),

				// Registered Claims
				new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var roles = await _userManager.GetRolesAsync(applicationUser);
			if (roles.Count > 0)
				foreach (var role in roles)
					claims.Add(new Claim(ClaimTypes.Role, role));

			var userClaims = await _userManager.GetClaimsAsync(applicationUser);
			if (userClaims.Count > 0)
				foreach (var userClaim in userClaims)
					claims.Add(userClaim);

			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecurityKey));
			var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _jwtSettings.Issuer,
				audience: _jwtSettings.Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
				signingCredentials: signingCredentials
				);

			return token;
		}
	}
}
