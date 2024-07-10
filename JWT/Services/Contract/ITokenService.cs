using JWT.Data.Models;
using System.IdentityModel.Tokens.Jwt;

namespace JWT.Services.Contract
{
	public interface ITokenService
	{
		Task<JwtSecurityToken> GenerateTokenAsync(ApplicationUser applicationUser);
	}
}
