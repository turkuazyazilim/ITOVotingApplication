using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Vote;
using ITOVotingApplication.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITOVotingApplication.Web.Controllers
{
	[Authorize(Roles = "İtop Kullanıcısı,SandikGorevlisi")]
	public class VoteController : Controller
	{
		private readonly IVoteService _voteService;
		private readonly IContactService _contactService;

		public VoteController(
			IVoteService voteService,
			IContactService contactService,
			ICompanyService companyService)
		{
			_voteService = voteService;
			_contactService = contactService;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var eligibleVoters = await _contactService.GetEligibleVotersAsync(1); // Default ballot box 1
			var voteResults = await _voteService.GetVoteResultsAsync(1);

			var model = new VoteDashboardViewModel
			{
				UserName = User.Identity.Name,
				FullName = User.FindFirst("FullName")?.Value,
				EligibleVoters = eligibleVoters.Data ?? new List<Core.DTOs.Contact.ContactDto>(),
				VoteResults = voteResults.Data,
				BallotBoxId = 1
			};

			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> CastVote(int contactId)
		{
			var contact = await _contactService.GetByIdAsync(contactId);

			if (!contact.Success)
			{
				TempData["Error"] = "Yetkili kişi bulunamadı.";
				return RedirectToAction(nameof(Index));
			}

			var model = new CastVoteViewModel
			{
				ContactId = contactId,
				ContactName = contact.Data.FullName,
				CompanyName = contact.Data.CompanyTitle,
				BallotBoxId = 1 // Default ballot box
			};

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CastVote(CastVoteViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

			var dto = new CastVoteDto
			{
				ContactId = model.ContactId,
				BallotBoxId = model.BallotBoxId,
				Description = model.Description
			};

			var result = await _voteService.CastVoteAsync(dto, userId);

			if (result.Success)
			{
				TempData["Success"] = "Oy başarıyla kaydedildi.";
				return RedirectToAction(nameof(Index));
			}

			ModelState.AddModelError(string.Empty, result.Message);
			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> Results()
		{
			var results = await _voteService.GetAllVoteResultsAsync();

			var model = new VoteResultsViewModel
			{
				Results = results.Data ?? new List<VoteResultDto>()
			};

			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> SearchContact(string identityNum)
		{
			if (string.IsNullOrWhiteSpace(identityNum))
			{
				return Json(new { success = false, message = "TC Kimlik No giriniz." });
			}

			var contact = await _contactService.GetByIdentityNumAsync(identityNum);

			if (contact.Success)
			{
				// Check if already voted
				var hasVoted = await _voteService.CheckIfVotedAsync(contact.Data.Id, 1);

				return Json(new
				{
					success = true,
					data = new
					{
						contact.Data.Id,
						contact.Data.FullName,
						contact.Data.CompanyTitle,
						contact.Data.EligibleToVote,
						HasVoted = hasVoted.Data
					}
				});
			}

			return Json(new { success = false, message = contact.Message });
		}
	}
}