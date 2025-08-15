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
			return View();
		}

		// API Endpoints for Dashboard Data
		[HttpGet]
		[Route("api/company/count")]
		public async Task<IActionResult> GetCompanyCount()
		{
			var result = await _companyService.GetCountAsync();
			return Ok(result);
		}

		[HttpGet]
		[Route("api/contact/count")]
		public async Task<IActionResult> GetContactCount()
		{
			var result = await _contactService.GetCountAsync();
			return Ok(result);
		}

		[HttpGet]
		[Route("api/vote/count")]
		public async Task<IActionResult> GetVoteCount()
		{
			var result = await _voteService.GetVoteCountAsync();
			return Ok(result);
		}

		[HttpGet]
		[Route("api/user/count")]
		public async Task<IActionResult> GetUserCount()
		{
			var result = await _userService.GetActiveUserCountAsync();
			return Ok(result);
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