using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITOVotingApplication.Web.Controllers
{
	[Authorize]
	public class DashboardController : Controller
	{
		private readonly ICompanyService _companyService;
		private readonly IContactService _contactService;
		private readonly IVoteService _voteService;
		private readonly IUserService _userService;
		private readonly IUnitOfWork _unitOfWork;

		public DashboardController(
			ICompanyService companyService,
			IContactService contactService,
			IVoteService voteService,
			IUserService userService,
			IUnitOfWork unitOfWork)
		{
			_companyService = companyService;
			_contactService = contactService;
			_voteService = voteService;
			_userService = userService;
			_unitOfWork = unitOfWork;
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
		[Route("api/contact/eligible-count")]
		public async Task<IActionResult> GetEligibleToVoteCount()
		{
			try
			{
				var result = await _contactService.GetCountAsync(true);
				return Ok(new { success = result.Success, data = result.Data, message = result.Message });
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, data = 0, message = "Oy kullanacak sayısı alınamadı" });
			}
		}

		[HttpGet]
		[Route("api/company/document-delivered-count")]
		public async Task<IActionResult> GetDocumentDeliveredCount()
		{
			try
			{
				var result = await _companyService.GetDocumentDeliveredCountAsync();
				return Ok(new { success = result.Success, data = result.Data, message = result.Message });
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, data = 0, message = "Belge teslim edilen firma sayısı alınamadı" });
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

		[HttpGet]
		[Route("api/dashboard/user-committees")]
		public async Task<IActionResult> GetUserCommittees()
		{
			try
			{
				var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
				if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
				{
					return Ok(new { success = false, message = "Kullanıcı bulunamadı", data = new List<object>() });
				}

				// Kullanıcının komitelerini al
				var userCommittees = await _unitOfWork.UserCommittees.Query()
					.Include(uc => uc.Committee)
					.Where(uc => uc.UserId == userId)
					.Select(uc => new
					{
						id = uc.CommitteeId,
						committeeNum = uc.Committee.CommitteeNum,
						committeeDescription = uc.Committee.CommitteeDescription,
						companyCount = _unitOfWork.Companies.Query()
							.Count(c => c.CommitteeId == uc.CommitteeId && c.IsActive)
					})
					.ToListAsync();

				return Ok(new { success = true, data = userCommittees });
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, message = $"Komiteler alınamadı: {ex.Message}", data = new List<object>() });
			}
		}

		[HttpGet]
		[Route("api/dashboard/committee-stats/{committeeId}")]
		public async Task<IActionResult> GetCommitteeStats(int committeeId)
		{
			try
			{
				// Komiteye bağlı firma sayısı
				var companyCount = await _unitOfWork.Companies.Query()
					.CountAsync(c => c.CommitteeId == committeeId && c.IsActive);

				// Komiteye bağlı firmalardaki yetkili sayısı
				var contactCount = await _unitOfWork.Contacts.Query()
					.CountAsync(c => c.Company.CommitteeId == committeeId && c.Company.IsActive);

				// Oy kullanabilecek yetkili sayısı
				var eligibleCount = await _unitOfWork.Contacts.Query()
					.CountAsync(c => c.Company.CommitteeId == committeeId && c.Company.IsActive && c.EligibleToVote);

				// Belge teslim edilmiş firma sayısı
				var documentDeliveredCount = await _unitOfWork.Companies.Query()
					.CountAsync(c => c.CommitteeId == committeeId && c.IsActive && (c.DocumentStatus == Core.Enums.DocumentStatus.OnaylanmisYetkiBelgesiYuklendi || c.DocumentStatus == Core.Enums.DocumentStatus.SahisSirketiOlarakKatilacak));

				return Ok(new
				{
					success = true,
					data = new
					{
						companyCount,
						contactCount,
						eligibleCount,
						documentDeliveredCount
					}
				});
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, message = $"İstatistikler alınamadı: {ex.Message}" });
			}
		}
	}
}