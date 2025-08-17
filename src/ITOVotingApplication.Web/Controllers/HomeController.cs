using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace VotingApplication.Web.Controllers
{
	public class HomeController : Controller
	{
		[Authorize]
		public IActionResult Index()
		{
			if (User.IsInRole("Admin"))
			{
				return RedirectToAction("Index", "Dashboard");
			}
			else if (User.IsInRole("SandikGorevlisi") || User.IsInRole("BallotOfficer"))
			{
				return RedirectToAction("Index", "Vote");
			}

			// Default view for other users
			return View();
		}

		[AllowAnonymous]
		public IActionResult Privacy()
		{
			return View();
		}

		[AllowAnonymous]
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View();
		}
	}
}