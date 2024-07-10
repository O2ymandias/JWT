using JWT.Data.Models.Dtos;

namespace JWT.Services.Contract
{
	public interface IAuthService
	{
		Task<AuthDto> RegisterUserAsync(RegisterDto model);
		Task<AuthDto> LoginUserAsync(LoginDto model);
		Task<string> AddRoleAsync(AddRoleDto model);
		Task<AuthDto> RefreshTokenAsync(string refreshToken);
		Task<bool> RevokeTokenAsync(string refreshToken);
	}
}
