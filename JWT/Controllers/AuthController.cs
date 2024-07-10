using JWT.Data.Models.Dtos;
using JWT.Services.Contract;
using Microsoft.AspNetCore.Mvc;

namespace JWT.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("Register")]
		public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.RegisterUserAsync(model);
			if (!result.IsAuthenticated)
				return BadRequest(result.Message);

			SetRefreshTokenToCookieStorage(result.RefreshToken!, result.RefreshTokenExpiration);
			return Ok(result);
		}

		[HttpPost("Login")]
		public async Task<IActionResult> LoginAsync([FromBody] LoginDto model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.LoginUserAsync(model);
			if (!result.IsAuthenticated)
				return BadRequest(result.Message);

			if (!string.IsNullOrEmpty(result.RefreshToken))
				SetRefreshTokenToCookieStorage(result.RefreshToken, result.RefreshTokenExpiration);

			return Ok(result);
		}

		[HttpPost("AddRole")]
		public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleDto model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.AddRoleAsync(model);
			return string.IsNullOrEmpty(result)
				? Ok(model)
				: BadRequest(result);
		}

		[HttpGet("RefreshToken")]
		public async Task<IActionResult> RefreshTokenAsync()
		{
			if (!Request.Cookies.ContainsKey("refreshToken"))
				return BadRequest("There is no refresh token provided");

			var refreshToken = Request.Cookies["refreshToken"]!;
			var result = await _authService.RefreshTokenAsync(refreshToken);

			if (!result.IsAuthenticated)
				return BadRequest(result.Message);

			if (!string.IsNullOrEmpty(result.RefreshToken))
				SetRefreshTokenToCookieStorage(result.RefreshToken, result.RefreshTokenExpiration);

			return Ok(result);
		}

		[HttpPost("RevokeToken")]
		public async Task<IActionResult> RevokeTokenAsync([FromBody] RevokeTokenDto revokeToken)
		{
			var refreshToken = revokeToken.RefreshToken ?? Request.Cookies["refreshToken"];
			if (string.IsNullOrEmpty(refreshToken))
				return BadRequest("Token is required!");

			var isRevokedSuccessfully = await _authService.RevokeTokenAsync(refreshToken);
			return isRevokedSuccessfully
				? Ok("Token successfully revoked")
				: BadRequest("Token is invalid");
		}


		private void SetRefreshTokenToCookieStorage(string refreshToken, DateTime refreshTokenExpiration)
		{
			var cookieOptions = new CookieOptions()
			{
				HttpOnly = true,
				Expires = refreshTokenExpiration.ToLocalTime()
			};
			Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
		}

	}
}
