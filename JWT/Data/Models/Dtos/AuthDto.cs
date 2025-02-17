﻿using System.Text.Json.Serialization;

namespace JWT.Data.Models.Dtos
{
	public class AuthDto
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public List<string> Roles { get; set; }
		public string Message { get; set; }
		public bool IsAuthenticated { get; set; }
		public string Token { get; set; }
		public DateTime TokenExpiresOn { get; set; }

		[JsonIgnore]
		public string? RefreshToken { get; set; }
		public DateTime RefreshTokenExpiration { get; set; }
	}
}
