using System.ComponentModel.DataAnnotations;

namespace JWT.Data.Models.Dtos
{
	public class RegisterDto
	{
		[Required, StringLength(50)]
		public string FirstName { get; set; }

		[Required, StringLength(50)]
		public string LastName { get; set; }

		[Required, StringLength(100)]
		public string Email { get; set; }

		[Required, StringLength(50)]
		public string UserName { get; set; }

		[Required, StringLength(50)]
		public string Password { get; set; }

	}
}
