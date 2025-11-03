using AutoMapper;
using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Company;
using ITOVotingApplication.Core.DTOs.Contact;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITOVotingApplication.Web.Controllers
{
    [Authorize(Roles = "Saha Görevlisi")]
    public class FieldWorkerController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IContactService _contactService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<FieldWorkerController> _logger;

        public FieldWorkerController(
            ICompanyService companyService,
            IContactService contactService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<FieldWorkerController> logger)
        {
            _companyService = companyService;
            _contactService = contactService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // Ana arama sayfası
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // AJAX firma arama API'si
        [HttpGet]
        public async Task<IActionResult> SearchCompanies([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 3)
                {
                    return Json(new { success = false, message = "En az 3 karakter girin" });
                }

                var searchRequest = new PagedRequest
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm.Trim()
                };

                var result = await _companyService.GetAllAsync(searchRequest);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        data = result.Data.Items,
                        totalItems = result.Data.TotalCount,
                        totalPages = result.Data.TotalPages,
                        currentPage = result.Data.PageNumber
                    });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firma arama hatası: {SearchTerm}", searchTerm);
                return Json(new { success = false, message = "Arama sırasında hata oluştu" });
            }
        }

        // Firma detaylarını getir
        [HttpGet]
        public async Task<IActionResult> GetCompanyDetails(int id)
        {
            try
            {
                var result = await _companyService.GetByIdAsync(id);

                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firma detay getirme hatası: {CompanyId}", id);
                return Json(new { success = false, message = "Firma detayları getirilemedi" });
            }
        }

        // Kullanıcı bilgilerini getir
        [HttpGet]
        public IActionResult GetUserInfo()
        {
            try
            {
                var fullName = User.FindFirst("FullName")?.Value ?? "Saha Görevlisi";
                var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
                var mobilePhone = User.FindFirst(System.Security.Claims.ClaimTypes.MobilePhone)?.Value ?? "";

                return Json(new {
                    success = true,
                    data = new {
                        fullName = fullName,
                        userName = userName,
                        email = email,
                        mobilePhone = mobilePhone
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı bilgisi getirme hatası");
                return Json(new { success = false, message = "Kullanıcı bilgisi getirilemedi" });
            }
        }

        // Firma güncelle
        [HttpPost]
        public async Task<IActionResult> UpdateCompany([FromBody] UpdateCompanyDto companyDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // Önce mevcut company'yi veritabanından al
                var existingCompany = await _unitOfWork.Companies.GetByIdAsync(companyDto.Id);
                if (existingCompany == null)
                {
                    return Json(new { success = false, message = "Firma bulunamadı" });
                }

                // Sadece güncellenebilir alanları güncelle
                existingCompany.Title = companyDto.Title;
                existingCompany.TradeRegistrationNumber = companyDto.TradeRegistrationNumber;
                existingCompany.RegistrationAddress = companyDto.RegistrationAddress;
                existingCompany.IsActive = companyDto.IsActive;
                existingCompany.Has2022AuthorizationCertificate = companyDto.Has2022AuthorizationCertificate;

                // TC firma için DocumentStatus güncellemesi
                existingCompany.DocumentStatus = companyDto.DocumentStatus;

                _unitOfWork.Companies.Update(existingCompany);
                await _unitOfWork.CompleteAsync();

                return Json(new { success = true, message = "Firma başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firma güncelleme hatası: {CompanyId}", companyDto.Id);
                return Json(new { success = false, message = "Firma güncellenirken hata oluştu" });
            }
        }

        // Yetkili kişi oluştur (Referanslar için)
        [HttpPost]
        public async Task<IActionResult> CreateContact([FromBody] CreateContactDto contactDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var result = await _contactService.CreateAsync(contactDto);

                if (result.Success)
                {
                    return Json(new { success = true, message = "Yetkili başarıyla oluşturuldu", data = result.Data });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yetkili oluşturma hatası: {CompanyId}, {AuthorizationType}", contactDto.CompanyId, contactDto.AuthorizationType);
                return Json(new { success = false, message = "Yetkili oluşturulurken hata oluştu" });
            }
        }

        // Yetkili kişi güncelle
        [HttpPost]
        public async Task<IActionResult> UpdateContact([FromBody] UpdateContactDto contactDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var result = await _contactService.UpdateAsync(contactDto);

                if (result.Success)
                {
                    return Json(new { success = true, message = "Yetkili başarıyla güncellendi", data = result.Data });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yetkili güncelleme hatası: {ContactId}", contactDto.Id);
                return Json(new { success = false, message = "Yetkili güncellenirken hata oluştu" });
            }
        }
    }
}