using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWT.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class SecuredController : ControllerBase
	{
		[HttpGet]
		public IActionResult GetData()
			=> Ok("Hello From SecuredController");
	}
}
