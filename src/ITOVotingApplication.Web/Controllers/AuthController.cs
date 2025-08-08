using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ITOVotingApplication.Web.Controllers
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

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
		{
			var result = await _authService.LoginAsync(loginDto);

			if (!result.Success)
				return BadRequest(result);

			return Ok(result);
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
		{
			var result = await _authService.RegisterAsync(registerDto);

			if (!result.Success)
				return BadRequest(result);

			return Ok(result);
		}

		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
		{
			var result = await _authService.RefreshTokenAsync(refreshToken);

			if (!result.Success)
				return BadRequest(result);

			return Ok(result);
		}
	}
}