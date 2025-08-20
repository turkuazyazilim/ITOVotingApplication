using ITOVotingApplication.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITOVotingApplication.Web.Controllers
{
	[Authorize]
	public class DashboardController : Controller
	{
		private readonly ICompanyService _companyService;
		private readonly IContactService _contactService;
		private readonly IVoteService _voteService;
		private readonly IUserService _userService;

		public DashboardController(
			ICompanyService companyService,
			IContactService contactService,
			IVoteService voteService,
			IUserService userService)
		{
			_companyService = companyService;
			_contactService = contactService;
			_voteService = voteService;
			_userService = userService;
		}

		[HttpGet]
		public IActionResult Index()
		{
			// User bilgilerini ViewBag'e koy
			if (User.Identity.IsAuthenticated)
			{
				ViewBag.FullName = User.FindFirst("FullName")?.Value ?? "";
				ViewBag.UserName = User.Identity.Name ?? "";
				ViewBag.UserRole = User.IsInRole("Admin") ? "Admin" : User.IsInRole("User") ? "User" : "Kullanıcı";
				ViewBag.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
			}
			
			return View();
		}

		// API Endpoints for Dashboard Data
		[HttpGet]
		[Route("api/company/count")]
		public async Task<IActionResult> GetCompanyCount()
		{
			try
			{
				var result = await _companyService.GetCountAsync(true);
				return Ok(new { success = result.Success, data = result.Data, message = result.Message });
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, data = 0, message = "Firma sayısı alınamadı" });
			}
		}

		[HttpGet]
		[Route("api/contact/count")]
		public async Task<IActionResult> GetContactCount()
		{
			try
			{
				var result = await _contactService.GetCountAsync();
				return Ok(new { success = result.Success, data = result.Data, message = result.Message });
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, data = 0, message = "Yetkili sayısı alınamadı" });
			}
		}

		[HttpGet]
		[Route("api/vote/count")]
		public async Task<IActionResult> GetVoteCount()
		{
			try
			{
				var result = await _voteService.GetVoteCountAsync();
				return Ok(new { success = result.Success, data = result.Data, message = result.Message });
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, data = 0, message = "Oy sayısı alınamadı" });
			}
		}

		[HttpGet]
		[Route("api/user/count")]
		public async Task<IActionResult> GetUserCount()
		{
			try
			{
				var result = await _userService.GetActiveUserCountAsync();
				return Ok(new { success = result.Success, data = result.Data, message = result.Message });
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, data = 0, message = "Kullanıcı sayısı alınamadı" });
			}
		}

		[HttpGet]
		[Route("api/dashboard/stats")]
		public async Task<IActionResult> GetDashboardStats()
		{
			var stats = new
			{
				Companies = await _companyService.GetCountAsync(),
				Contacts = await _contactService.GetCountAsync(),
				Votes = await _voteService.GetVoteCountAsync(),
				Users = await _userService.GetActiveUserCountAsync()
			};

			return Ok(new { success = true, data = stats });
		}

		[HttpGet]
		[Route("api/dashboard/activities")]
		public async Task<IActionResult> GetRecentActivities()
		{
			// Implement recent activities logic
			var activities = new[]
			{
				new { icon = "user-plus", color = "green", text = "Yeni firma kaydı", time = "5 dakika önce" },
				new { icon = "check-square", color = "blue", text = "Oy kullanıldı", time = "12 dakika önce" }
			};

			return Ok(new { success = true, data = activities });
		}
	}
}