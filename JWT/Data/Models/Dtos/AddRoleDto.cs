using System.ComponentModel.DataAnnotations;

namespace JWT.Data.Models.Dtos
{
	public class AddRoleDto
	{
		[Required]
		public string UserId { get; set; }
		[Required]
		public string RoleName { get; set; }
	}
}
