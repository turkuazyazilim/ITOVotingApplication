using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Company;
using ITOVotingApplication.Core.DTOs.Contact;
using ITOVotingApplication.Core.DTOs.User;
using ITOVotingApplication.Core.Interfaces;
using ITOVotingApplication.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VotingApplication.Web.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
		private readonly ICompanyService _companyService;
		private readonly IContactService _contactService;
		private readonly IUserService _userService;
		private readonly IUnitOfWork _unitOfWork;

		public AdminController(
			ICompanyService companyService,
			IContactService contactService,
			IUserService userService,
			IUnitOfWork unitOfWork)
		{
			_companyService = companyService;
			_contactService = contactService;
			_userService = userService;
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			//var model = new AdminDashboardViewModel
			//{
			//	UserName = User.Identity.Name,
			//	FullName = User.FindFirst("FullName")?.Value
			//};
			//return View(model);
			return null;
		}

		// Company Management
		[HttpGet]
		public async Task<IActionResult> Companies(int page = 1, string search = "")
		{
			var request = new PagedRequest
			{
				PageNumber = page,
				PageSize = 10,
				SearchTerm = search
			};

			var result = await _companyService.GetAllAsync(request);

			if (result.Success)
			{
				var model = new CompanyListViewModel
				{
					Companies = result.Data,
					SearchTerm = search
				};
				return View(model);
			}

			TempData["Error"] = result.Message;
			return View(new CompanyListViewModel());
		}

		[HttpGet]
		public async Task<IActionResult> CreateCompany()
		{
			await PrepareViewBag();

			return View(new CreateCompanyViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateCompany(CreateCompanyViewModel model)
		{
			if (!ModelState.IsValid)
			{
				// Reload dropdowns if validation fails
				await PrepareViewBag();
				return View(model);
			}

			var dto = new CreateCompanyDto
			{
				RegistrationNumber = model.RegistrationNumber,
				Title = model.Title,
				TradeRegistrationNumber = model.TradeRegistrationNumber,
				RegistrationAddress = model.RegistrationAddress,
				ProfessionalGroup = model.ProfessionalGroup,
				OfficePhone = model.OfficePhone,
				MobilePhone = model.MobilePhone,
				Email = model.Email,
				IsActive = model.IsActive
			};

			//var result = await _companyService.CreateAsync(dto);

			//if (result.Success)
			//{
			//	TempData["Success"] = "Firma başarıyla oluşturuldu.";
			//	return RedirectToAction(nameof(Companies));
			//}

			//await PrepareViewBag();
			return View(model);
		}

		// User Management
		[HttpGet]
		public async Task<IActionResult> Users(int page = 1, string search = "")
		{
			var request = new PagedRequest
			{
				PageNumber = page,
				PageSize = 10,
				SearchTerm = search
			};

			var result = await _userService.GetAllAsync(request);

			if (result.Success)
			{
				var model = new UserListViewModel
				{
					Users = result.Data,
					SearchTerm = search
				};
				return View(model);
			}

			TempData["Error"] = result.Message;
			return View(new UserListViewModel());
		}

		[HttpGet]
		public IActionResult CreateUser()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateUser(CreateUserViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var dto = new CreateUserDto
			{
				UserName = model.UserName,
				Password = model.Password,
				Email = model.Email,
				PhoneNumber = model.PhoneNumber,
				FirstName = model.FirstName,
				LastName = model.LastName,
				RoleIds = model.SelectedRoleIds
			};

			var result = await _userService.CreateAsync(dto);

			if (result.Success)
			{
				TempData["Success"] = "Kullanıcı başarıyla oluşturuldu.";
				return RedirectToAction(nameof(Users));
			}

			ModelState.AddModelError(string.Empty, result.Message);
			return View(model);
		}

		// Contact Management
		[HttpGet]
		public async Task<IActionResult> CreateContact()
		{
			await PrepareContactViewBag();
			return View(new CreateContactViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateContact(CreateContactViewModel model)
		{
			if (!ModelState.IsValid)
			{
				await PrepareContactViewBag();
				return View(model);
			}

			var dto = new CreateContactDto
			{
				CompanyId = model.CompanyId,
				FirstName = model.FirstName,
				LastName = model.LastName,
				IdentityNum = model.IdentityNum,
				AuthorizationType = model.AuthorizationType,
				MobilePhone = model.MobilePhone,
				Email = model.Email,
				EligibleToVote = model.EligibleToVote
			};

			var result = await _contactService.CreateAsync(dto);

			if (result.Success)
			{
				TempData["Success"] = "Yetkili kişi başarıyla oluşturuldu.";
				return RedirectToAction(nameof(Contacts));
			}

			ModelState.AddModelError(string.Empty, result.Message);
			await PrepareContactViewBag();
			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> Contacts(int page = 1, string search = "")
		{
			var request = new PagedRequest
			{
				PageNumber = page,
				PageSize = 10,
				SearchTerm = search
			};

			var result = await _contactService.GetAllAsync(request);

			if (result.Success)
			{
				var model = new ContactListViewModel
				{
					Contacts = result.Data,
					SearchTerm = search
				};
				return View(model);
			}

			TempData["Error"] = result.Message;
			return View(new ContactListViewModel());
		}
		private async Task PrepareContactViewBag()
		{
			// Firmaları yükle
			var companiesRequest = new PagedRequest
			{
				PageSize = 100,
				PageNumber = 1
			};

			var companiesResult = await _companyService.GetAllAsync(companiesRequest);

			ViewBag.Companies = companiesResult.Success && companiesResult.Data != null
				? companiesResult.Data.Items.Select(c => new SelectListItem
				{
					Value = c.Id.ToString(),
					Text = $"{c.RegistrationNumber} - {c.Title}"
				}).ToList()
				: new List<SelectListItem>();

			// Komiteleri yükle  
			var committees = await _unitOfWork.Committees.GetAllAsync();
			ViewBag.Committees = committees.Select(c => new SelectListItem
			{
				Value = c.Id.ToString(),
				Text = c.CommitteeDescription
			}).ToList();

			// Yetki tiplerini yükle
			ViewBag.AuthorizationTypes = new List<SelectListItem>
	{
		new SelectListItem { Value = "1", Text = "Firma Yetkilisi" },
		new SelectListItem { Value = "2", Text = "Vekil" },
		new SelectListItem { Value = "3", Text = "Temsilci" }
	};
		}
		private async Task PrepareViewBag()
		{
			ViewBag.ProfessionalGroups = new List<SelectListItem>
			{
				new SelectListItem { Value = "5", Text = "5 - BİLGİ TEKNOLOJİLERİ" }
			};
		}
	}
}