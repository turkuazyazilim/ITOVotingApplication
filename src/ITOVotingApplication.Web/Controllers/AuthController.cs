using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
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
		[AllowAnonymous]
		public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
		{
			var result = await _authService.LoginAsync(loginDto);

			if (result.Success)
			{
				return Ok(result);
			}

			return BadRequest(result);
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

		[HttpPost("logout")]
		[Authorize]
		public async Task<IActionResult> Logout()
		{
			var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (int.TryParse(userId, out int id))
			{
				await _authService.LogoutAsync(id);
			}

			return Ok(new { success = true, message = "Çıkış başarılı" });
		}

		[HttpGet("check")]
		[Authorize]
		public IActionResult CheckAuth()
		{
			return Ok(new
			{
				success = true,
				user = User.Identity.Name,
				isAuthenticated = User.Identity.IsAuthenticated
			});
		}
	}
}