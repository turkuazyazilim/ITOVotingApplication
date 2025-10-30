using AutoMapper;
using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Business.Services.Interfaces;
using ITOVotingApplication.Core.DTOs.Committee;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Company;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using ITOVotingApplication.Core.Enums;
using ITOVotingApplication.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ITOVotingApplication.Web.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class CompanyController : Controller
	{
		private readonly ICommitteeService _committeeService;
		private readonly ICompanyService _companyService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public CompanyController(ICommitteeService committeeService, ICompanyService companyService, IMapper mapper, IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
		{
			_committeeService = committeeService;
			_companyService = companyService;
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}

		[HttpGet("dropdown")]
		public async Task<IActionResult> GetForDropdown()
		{
			var result = await _committeeService.GetAllForDropdownAsync();
			return Ok(result);
		}

		[HttpGet("committees")]
		public async Task<IActionResult> GetCommittees()
		{
			var result = await _committeeService.GetAllForDropdownAsync();
			return Ok(result);
		}

		[HttpGet("")]
		public async Task<IActionResult> GetCompanies([FromQuery] PagedRequest? request)
		{
			try
			{
				// Create new request if null or set defaults
				request ??= new PagedRequest();
				
				if (request.PageSize <= 0) request.PageSize = 500;
				if (request.PageNumber <= 0) request.PageNumber = 1;
				request.SearchTerm ??= "";
				request.SortBy ??= "";

				var result = await _companyService.GetAllAsync(request);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, message = "Firmalar yüklenirken hata oluştu", data = new { items = new List<object>() } });
			}
		}
		[HttpGet("{id:int}")]
		public async Task<ApiResponse<CompanyDto>> GetByIdAsync(int id)
		{
			try
			{
				var company = await _unitOfWork.Companies.Query()
					.Include(c => c.ActiveContact)
					.FirstOrDefaultAsync(c => c.Id == id);

				if (company == null)
				{
					return ApiResponse<CompanyDto>.ErrorResult("Firma bulunamadı.");
				}

				var result = _mapper.Map<CompanyDto>(company);
				return ApiResponse<CompanyDto>.SuccessResult(result);
			}
			catch (Exception ex)
			{
				return ApiResponse<CompanyDto>.ErrorResult($"Firma getirme hatası: {ex.Message}");
			}
		}
		[HttpGet("statistics")]
		public async Task<ApiResponse<CompanyStatisticsDto>> GetStatisticsAsync()
		{
			try
			{
				var statistics = new CompanyStatisticsDto
				{
					TotalCompanies = await _unitOfWork.Companies.Query().CountAsync(),
					ActiveCompanies = await _unitOfWork.Companies.Query().Where(c => c.IsActive).CountAsync(),
					InactiveCompanies = await _unitOfWork.Companies.Query().Where(c => !c.IsActive).CountAsync(),


					// Son eklenen firmalar
					RecentlyAdded = await _unitOfWork.Companies.Query()
						.OrderByDescending(c => c.Id)
						.Take(5)
						.Select(c => new { c.Title, c.RegistrationNumber })
						.ToListAsync()
				};

				return ApiResponse<CompanyStatisticsDto>.SuccessResult(statistics);
			}
			catch (Exception ex)
			{
				return ApiResponse<CompanyStatisticsDto>.ErrorResult($"İstatistik getirme hatası: {ex.Message}");
			}
		}

		[HttpPost]
		public async Task<ApiResponse<CompanyDto>> CreateAsync(CreateCompanyDto dto)
		{
			try
			{
				// Check for duplicate registration number
				var existingCompany = await _unitOfWork.Companies
					.SingleOrDefaultAsync(c => c.RegistrationNumber == dto.RegistrationNumber);

				if (existingCompany != null)
				{
					return ApiResponse<CompanyDto>.ErrorResult("Bu sicil numarası ile kayıtlı firma bulunmaktadır.");
				}


				var company = _mapper.Map<Company>(dto);
				company.IsActive = true;

				await _unitOfWork.Companies.AddAsync(company);
				await _unitOfWork.CompleteAsync();

				var createdCompany = await _unitOfWork.Companies.Query()
					.FirstOrDefaultAsync(c => c.Id == company.Id);

				var result = _mapper.Map<CompanyDto>(createdCompany);
				return ApiResponse<CompanyDto>.SuccessResult(result, "Firma başarıyla oluşturuldu.");
			}
			catch (Exception ex)
			{
				return ApiResponse<CompanyDto>.ErrorResult($"Firma oluşturma hatası: {ex.Message}");
			}
		}

		[HttpPut("{id:int}")]
		public async Task<ApiResponse<CompanyDto>> UpdateAsync(int id, UpdateCompanyDto dto)
		{
			try
			{
				// Set the ID from route parameter
				dto.Id = id;
				
				var company = await _unitOfWork.Companies.GetByIdAsync(id);

				if (company == null)
				{
					return ApiResponse<CompanyDto>.ErrorResult("Firma bulunamadı.");
				}

				// Check for duplicate registration number
				var existingCompany = await _unitOfWork.Companies
					.SingleOrDefaultAsync(c => c.RegistrationNumber == dto.RegistrationNumber && c.Id != id);

				if (existingCompany != null)
				{
					return ApiResponse<CompanyDto>.ErrorResult("Bu sicil numarası başka bir firmada kullanılmaktadır.");
				}

				// Validate active contact if provided
				if (dto.ActiveContactId.HasValue)
				{
					var contact = await _unitOfWork.Contacts
						.SingleOrDefaultAsync(c => c.Id == dto.ActiveContactId.Value && c.CompanyId == id);

					if (contact == null)
					{
						return ApiResponse<CompanyDto>.ErrorResult("Geçersiz yetkili kişi.");
					}
				}

				_mapper.Map(dto, company);
				_unitOfWork.Companies.Update(company);
				await _unitOfWork.CompleteAsync();

				var updatedCompany = await _unitOfWork.Companies.Query()
					.Include(c => c.ActiveContact)
					.FirstOrDefaultAsync(c => c.Id == company.Id);

				var result = _mapper.Map<CompanyDto>(updatedCompany);
				return ApiResponse<CompanyDto>.SuccessResult(result, "Firma başarıyla güncellendi.");
			}
			catch (Exception ex)
			{
				return ApiResponse<CompanyDto>.ErrorResult($"Firma güncelleme hatası: {ex.Message}");
			}
		}

		[HttpDelete("{id:int}")]
		public async Task<ApiResponse<bool>> DeleteAsync(int id)
		{
			try
			{
				var company = await _unitOfWork.Companies.GetByIdAsync(id);

				if (company == null)
				{
					return ApiResponse<bool>.ErrorResult("Firma bulunamadı.");
				}

				// Check if company has any votes
				var hasVotes = await _unitOfWork.VoteTransactions
					.Query()
					.AnyAsync(v => v.CompanyId == id);

				if (hasVotes)
				{
					return ApiResponse<bool>.ErrorResult("Oy kaydı bulunan firma silinemez.");
				}

				// Soft delete
				company.IsActive = false;
				_unitOfWork.Companies.Update(company);
				await _unitOfWork.CompleteAsync();

				return ApiResponse<bool>.SuccessResult(true, "Firma başarıyla silindi.");
			}
			catch (Exception ex)
			{
				return ApiResponse<bool>.ErrorResult($"Firma silme hatası: {ex.Message}");
			}
		}

		[HttpGet("active")]
		public async Task<ApiResponse<List<CompanyDto>>> GetActiveCompaniesAsync()
		{
			try
			{
				var companies = await _unitOfWork.Companies.Query()
					.Include(c => c.ActiveContact)
					.Where(c => c.IsActive)
					.OrderBy(c => c.Title)
					.ToListAsync();

				var result = _mapper.Map<List<CompanyDto>>(companies);
				return ApiResponse<List<CompanyDto>>.SuccessResult(result);
			}
			catch (Exception ex)
			{
				return ApiResponse<List<CompanyDto>>.ErrorResult($"Aktif firma listesi getirme hatası: {ex.Message}");
			}
		}

		[HttpGet("registration/{registrationNumber}")]
		public async Task<ApiResponse<CompanyDto>> GetByRegistrationNumberAsync(string registrationNumber)
		{
			try
			{
				var company = await _unitOfWork.Companies.Query()
					.Include(c => c.ActiveContact)
					.FirstOrDefaultAsync(c => c.RegistrationNumber == registrationNumber);

				if (company == null)
				{
					return ApiResponse<CompanyDto>.ErrorResult("Firma bulunamadı.");
				}

				var result = _mapper.Map<CompanyDto>(company);
				return ApiResponse<CompanyDto>.SuccessResult(result);
			}
			catch (Exception ex)
			{
				return ApiResponse<CompanyDto>.ErrorResult($"Firma getirme hatası: {ex.Message}");
			}
		}
		public class CompanyStatisticsDto
		{
			public int TotalCompanies { get; set; }
			public int ActiveCompanies { get; set; }
			public int InactiveCompanies { get; set; }
				public object RecentlyAdded { get; set; }
		}


		[HttpGet("{id:int}/generate-authorization-document")]
		public async Task<IActionResult> GenerateAuthorizationDocument(int id)
		{
			try
			{
				// Firma bilgilerini getir
				var company = await _unitOfWork.Companies.GetByIdAsync(id);
				if (company == null)
				{
					return NotFound(new { message = "Firma bulunamadı." });
				}

				var userFirstLastName = User.Claims.FirstOrDefault(w => w.Type == "FullName");
				var userPhoneNumber = User.Claims.FirstOrDefault(w => w.Type == $"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone");

				// Template dosyasının yolunu oluştur
				string templatePath = Path.Combine(_webHostEnvironment.WebRootPath ?? _webHostEnvironment.ContentRootPath, 
					"Documents", "yetkidilekcesi.docx");

				if (!System.IO.File.Exists(templatePath))
				{
					return NotFound(new { message = "Template dosyası bulunamadı." });
				}

				// Geçici dosya adı oluştur
				string fileName = $"Yetki_Belgesi_Talep_Dilekcesi_{company.RegistrationNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
				string tempPath = Path.Combine(Path.GetTempPath(), fileName);

				// Template dosyasını kopyala
				System.IO.File.Copy(templatePath, tempPath, true);

				// Word belgesini düzenle
				using (WordprocessingDocument doc = WordprocessingDocument.Open(tempPath, true))
				{
					var body = doc.MainDocumentPart?.Document?.Body;
					if (body != null)
					{
						// Tüm text elementlerini bul ve placeholder'ları değiştir
						var textElements = body.Descendants<Text>().ToList();
						
						foreach (var text in textElements)
						{
							if (text.Text != null)
							{
								// {{RegistrationNumber}} placeholder'ını sicil numarası ile değiştir
								if (text.Text.Contains("RegistrationNumber"))
								{
									text.Text = text.Text.Replace("RegistrationNumber", company.RegistrationNumber);
								}
								
								// {{Title}} placeholder'ını firma adı ile değiştir
								if (text.Text.Contains("{{Title}}"))
								{
									text.Text = text.Text.Replace("{{Title}}", company.Title);
								}

								if (text.Text.Contains("ProfessionalGroup"))
								{
									text.Text = text.Text.Replace("ProfessionalGroup", company.ProfessionalGroup);
								}

								if (text.Text.Contains("UserFirstLastName"))
								{
									text.Text = text.Text.Replace("UserFirstLastName", userFirstLastName?.Value.ToString());
								}
								// PhoneNumber eklenecek
								if (text.Text.Contains("UserPhoneNumber"))
								{
									text.Text = text.Text.Replace("UserPhoneNumber", userPhoneNumber?.Value.ToString());
								}
							}
						}
					}

					doc.Save();
				}

				// Dosyayı döndür
				var fileBytes = await System.IO.File.ReadAllBytesAsync(tempPath);
				
				// Geçici dosyayı sil
				System.IO.File.Delete(tempPath);

				return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = $"Belge oluşturma hatası: {ex.Message}" });
			}
		}

		[HttpGet("{id:int}/generate-document-link")]
		public async Task<IActionResult> GenerateDocumentLink(int id)
		{
			try
			{
				// Firma bilgilerini getir
				var company = await _unitOfWork.Companies.GetByIdAsync(id);
				if (company == null)
				{
					return NotFound(new { message = "Firma bulunamadı." });
				}

				var userFirstLastName = User.Claims.FirstOrDefault(w => w.Type == "FullName");
				var userPhoneNumber = User.Claims.FirstOrDefault(w => w.Type == "MobilePhone");

				// Template dosyasının yolunu oluştur
				string templatePath = Path.Combine(_webHostEnvironment.WebRootPath ?? _webHostEnvironment.ContentRootPath, 
					"Documents", "yetkidilekcesi.docx");

				if (!System.IO.File.Exists(templatePath))
				{
					return NotFound(new { message = "Template dosyası bulunamadı." });
				}

				// Geçici dosya klasörü oluştur (wwwroot/temp)
				string tempFolderPath = Path.Combine(_webHostEnvironment.WebRootPath ?? _webHostEnvironment.ContentRootPath, "temp");
				if (!Directory.Exists(tempFolderPath))
				{
					Directory.CreateDirectory(tempFolderPath);
				}

				// Benzersiz dosya adı oluştur
				string uniqueId = Guid.NewGuid().ToString("N")[..8];
				string fileName = $"Yetki_Belgesi_{company.RegistrationNumber}_{uniqueId}.docx";
				string tempFilePath = Path.Combine(tempFolderPath, fileName);

				// Template dosyasını kopyala
				System.IO.File.Copy(templatePath, tempFilePath, true);

				// Word belgesini düzenle
				using (WordprocessingDocument doc = WordprocessingDocument.Open(tempFilePath, true))
				{
					var body = doc.MainDocumentPart?.Document?.Body;
					if (body != null)
					{
						// Tüm text elementlerini bul ve placeholder'ları değiştir
						var textElements = body.Descendants<Text>().ToList();
						
						foreach (var text in textElements)
						{
							if (text.Text != null)
							{
								if (text.Text.Contains("RegistrationNumber"))
								{
									text.Text = text.Text.Replace("RegistrationNumber", company.RegistrationNumber);
								}
								
								if (text.Text.Contains("{{Title}}"))
								{
									text.Text = text.Text.Replace("{{Title}}", company.Title);
								}

								if (text.Text.Contains("ProfessionalGroup"))
								{
									text.Text = text.Text.Replace("ProfessionalGroup", company.ProfessionalGroup);
								}

								if (text.Text.Contains("UserFirstLastName"))
								{
									text.Text = text.Text.Replace("UserFirstLastName", userFirstLastName?.Value.ToString());
								}

								if (text.Text.Contains("UserPhoneNumber"))
								{
									text.Text = text.Text.Replace("UserPhoneNumber", userPhoneNumber?.Value.ToString());
								}
							}
						}
					}

					doc.Save();
				}

				// 30 dakika sonra silinmek üzere dosyayı işaretle (basit yaklaşım için şimdilik manuel)
				// Gerçek uygulamada background service kullanılabilir

				// İndirme linkini oluştur
				var request = HttpContext.Request;
				string baseUrl = $"{request.Scheme}://{request.Host}";
				string downloadUrl = $"{baseUrl}/temp/{fileName}";

				return Ok(new { 
					success = true, 
					downloadUrl = downloadUrl,
					fileName = $"Yetki_Belgesi_{company.Title}_{DateTime.Now:yyyy-MM-dd}.docx",
					expiresIn = "30 dakika"
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = $"Link oluşturma hatası: {ex.Message}" });
			}
		}

		[HttpPost("{id:int}/upload-signed-document")]
		public async Task<IActionResult> UploadSignedDocument(int id, IFormFile signedDocument)
		{
			try
			{
				// Firma bilgilerini getir
				var company = await _unitOfWork.Companies.GetByIdAsync(id);
				if (company == null)
				{
					return NotFound(new { success = false, message = "Firma bulunamadı." });
				}

				// Dosya kontrolü
				if (signedDocument == null || signedDocument.Length == 0)
				{
					return BadRequest(new { success = false, message = "Dosya seçilmedi." });
				}

				// Dosya boyutu kontrolü (max 10MB)
				if (signedDocument.Length > 10 * 1024 * 1024)
				{
					return BadRequest(new { success = false, message = "Dosya boyutu 10MB'dan büyük olamaz." });
				}

				// Dosya uzantısı kontrolü
				var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
				var fileExtension = Path.GetExtension(signedDocument.FileName).ToLowerInvariant();
				if (!allowedExtensions.Contains(fileExtension))
				{
					return BadRequest(new { success = false, message = "Sadece PDF, DOC ve DOCX dosyaları desteklenmektedir." });
				}

				// Documents klasörü path'i
				string documentsPath = Path.Combine(_webHostEnvironment.WebRootPath ?? _webHostEnvironment.ContentRootPath, "Documents");
				if (!Directory.Exists(documentsPath))
				{
					Directory.CreateDirectory(documentsPath);
				}

				// Firma sicil numarasına göre klasör oluştur
				string companyFolderPath = Path.Combine(documentsPath, company.RegistrationNumber);
				if (!Directory.Exists(companyFolderPath))
				{
					Directory.CreateDirectory(companyFolderPath);
				}

				// Dosya adını oluştur: FirmaSicilNo_ImzalıYetkiBelgeDilekcesi.uzantı
				string fileName = $"{company.RegistrationNumber}_ImzalıYetkiBelgeDilekcesi{fileExtension}";
				string filePath = Path.Combine(companyFolderPath, fileName);

				// Eğer aynı isimde dosya varsa, eskisini backup olarak sakla
				if (System.IO.File.Exists(filePath))
				{
					string backupFileName = $"{company.RegistrationNumber}_ImzalıYetkiBelgeDilekcesi_Backup_{DateTime.Now:yyyyMMdd_HHmmss}{fileExtension}";
					string backupFilePath = Path.Combine(companyFolderPath, backupFileName);
					System.IO.File.Move(filePath, backupFilePath);
				}

				// Dosyayı kaydet
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await signedDocument.CopyToAsync(stream);
				}

				company.DocumentStatus = DocumentStatus.YetkiBelgesiYuklendi;

				_unitOfWork.Companies.Update(company);
				await _unitOfWork.CompleteAsync();

				return Ok(new { 
					success = true, 
					message = "İmzalı belge başarıyla yüklendi.",
					fileName = fileName,
					filePath = $"/Documents/{company.RegistrationNumber}/{fileName}"
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = $"Dosya yükleme hatası: {ex.Message}" });
			}
		}

		[HttpPost("{id:int}/upload-approved-document")]
		public async Task<IActionResult> UploadApprovedDocument(int id, [FromForm] IFormFile approvedDocument, [FromForm] string note = "")
		{
			try
			{
				// Firma bilgilerini getir
				var company = await _unitOfWork.Companies.GetByIdAsync(id);
				if (company == null)
				{
					return NotFound(new { success = false, message = "Firma bulunamadı." });
				}

				// Dosya kontrolü
				if (approvedDocument == null || approvedDocument.Length == 0)
				{
					return BadRequest(new { success = false, message = "Dosya seçilmedi." });
				}

				// Dosya boyutu kontrolü (max 10MB)
				if (approvedDocument.Length > 10 * 1024 * 1024)
				{
					return BadRequest(new { success = false, message = "Dosya boyutu 10MB'dan büyük olamaz." });
				}

				// Dosya uzantısı kontrolü - JPG, JPEG, PNG de destekleniyor
				var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
				var fileExtension = Path.GetExtension(approvedDocument.FileName).ToLowerInvariant();
				if (!allowedExtensions.Contains(fileExtension))
				{
					return BadRequest(new { success = false, message = "Sadece PDF, DOC, DOCX, JPG, JPEG ve PNG dosyaları desteklenmektedir." });
				}

				// Documents klasörü path'i
				string documentsPath = Path.Combine(_webHostEnvironment.WebRootPath ?? _webHostEnvironment.ContentRootPath, "Documents");
				if (!Directory.Exists(documentsPath))
				{
					Directory.CreateDirectory(documentsPath);
				}

				// Firma sicil numarasına göre klasör oluştur
				string companyFolderPath = Path.Combine(documentsPath, company.RegistrationNumber);
				if (!Directory.Exists(companyFolderPath))
				{
					Directory.CreateDirectory(companyFolderPath);
				}

				// Dosya adını oluştur: FirmaSicilNo_ITOYetkiBelgesi.uzantı
				string fileName = $"{company.RegistrationNumber}_ITOYetkiBelgesi{fileExtension}";
				string filePath = Path.Combine(companyFolderPath, fileName);

				// Eğer aynı isimde dosya varsa, eskisini backup olarak sakla
				if (System.IO.File.Exists(filePath))
				{
					string backupFileName = $"{company.RegistrationNumber}_ITOYetkiBelgesi_Backup_{DateTime.Now:yyyyMMdd_HHmmss}{fileExtension}";
					string backupFilePath = Path.Combine(companyFolderPath, backupFileName);
					System.IO.File.Move(filePath, backupFilePath);
				}

				// Dosyayı kaydet
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await approvedDocument.CopyToAsync(stream);
				}

				// Log not bilgisini
				if (!string.IsNullOrWhiteSpace(note))
				{
					Console.WriteLine($"Onaylı belge yüklendi - Firma: {company.RegistrationNumber}, Not: {note}");
				}

				return Ok(new {
					success = true,
					message = "Onaylı belge başarıyla yüklendi.",
					fileName = fileName,
					filePath = $"/Documents/{company.RegistrationNumber}/{fileName}",
					note = note ?? ""
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = $"Dosya yükleme hatası: {ex.Message}" });
			}
		}

		[HttpGet("{id:int}/documents")]
		public async Task<IActionResult> GetCompanyDocuments(int id)
		{
			try
			{
				// Firma bilgilerini getir
				var company = await _unitOfWork.Companies.GetByIdAsync(id);
				if (company == null)
				{
					return NotFound(new { success = false, message = "Firma bulunamadı." });
				}

				var documents = new List<object>();

				// Documents klasörü path'i
				string documentsPath = Path.Combine(_webHostEnvironment.WebRootPath ?? _webHostEnvironment.ContentRootPath, "Documents");
				string companyFolderPath = Path.Combine(documentsPath, company.RegistrationNumber);

				// Demo belgeler (sabit liste)
				var demoDocuments = new[]
				{
					new { type = "registration", name = "Yetki Belgesi Talep Dilekçesi", fileName = "template.docx", size = "45 KB", status = "template" },
					new { type = "authorization", name = "Onaylanmış Yetki Belgesi", fileName = "", size = "", status = "pending" }
				};

				foreach (var demo in demoDocuments)
				{
					var document = new
					{
						id = documents.Count + 1,
						type = demo.type,
						name = demo.name,
						fileName = demo.fileName,
						size = demo.size,
						uploadDate = DateTime.Now.AddDays(-10),
						status = demo.status,
						description = demo.type == "registration" ? "İTO üyelik yetki belgesi talep dilekçesi" : "İTO tarafından onaylanmış yetki belgesi"
					};

					// Eğer onaylanmış yetki belgesi ise, gerçek dosyayı kontrol et
					if (demo.type == "authorization" && Directory.Exists(companyFolderPath))
					{
						var possibleExtensions = new[] { "pdf", "jpg", "jpeg", "png", "doc", "docx" };
						foreach (var ext in possibleExtensions)
						{
							var filePath = Path.Combine(companyFolderPath, $"{company.RegistrationNumber}_ITOYetkiBelgesi.{ext}");
							if (System.IO.File.Exists(filePath))
							{
								var fileInfo = new FileInfo(filePath);
								document = new
								{
									id = documents.Count + 1,
									type = demo.type,
									name = demo.name,
									fileName = $"{company.RegistrationNumber}_ITOYetkiBelgesi.{ext}",
									size = FormatFileSize(fileInfo.Length),
									uploadDate = fileInfo.CreationTime,
									status = "uploaded",
									description = "İTO tarafından onaylanmış yetki belgesi"
								};
								break;
							}
						}
					}

					// İmzalı belgeyi kontrol et
					if (demo.type == "registration" && Directory.Exists(companyFolderPath))
					{
						var possibleExtensions = new[] { "pdf", "jpg", "jpeg", "png", "doc", "docx" };
						foreach (var ext in possibleExtensions)
						{
							var filePath = Path.Combine(companyFolderPath, $"{company.RegistrationNumber}_ImzalıYetkiBelgeDilekcesi.{ext}");
							if (System.IO.File.Exists(filePath))
							{
								var fileInfo = new FileInfo(filePath);
								document = new
								{
									id = documents.Count + 1,
									type = demo.type,
									name = demo.name,
									fileName = $"{company.RegistrationNumber}_ImzalıYetkiBelgeDilekcesi.{ext}",
									size = FormatFileSize(fileInfo.Length),
									uploadDate = fileInfo.CreationTime,
									status = "uploaded",
									description = "İmzalanmış yetki belgesi talep dilekçesi"
								};
								break;
							}
						}
					}

					documents.Add(document);
				}

				return Ok(new { success = true, documents = documents });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = $"Belge listesi getirme hatası: {ex.Message}" });
			}
		}

		private string FormatFileSize(long bytes)
		{
			string[] sizes = { "B", "KB", "MB", "GB" };
			int order = 0;
			double size = bytes;
			while (size >= 1024 && order < sizes.Length - 1)
			{
				order++;
				size = size / 1024;
			}
			return $"{size:0.##} {sizes[order]}";
		}

		[HttpDelete("{id:int}/document/{documentType}")]
		public async Task<IActionResult> DeleteCompanyDocument(int id, string documentType)
		{
			try
			{
				// Firma bilgilerini getir
				var company = await _unitOfWork.Companies.GetByIdAsync(id);
				if (company == null)
				{
					return NotFound(new { success = false, message = "Firma bulunamadı." });
				}

				// Geçerli belge türü kontrolü
				var validTypes = new[] { "registration", "authorization" };
				if (!validTypes.Contains(documentType))
				{
					return BadRequest(new { success = false, message = "Geçersiz belge türü." });
				}

				// Dosya path'ini oluştur
				string documentsPath = Path.Combine(_webHostEnvironment.WebRootPath ?? _webHostEnvironment.ContentRootPath, "Documents");
				string companyFolderPath = Path.Combine(documentsPath, company.RegistrationNumber);

				if (!Directory.Exists(companyFolderPath))
				{
					return NotFound(new { success = false, message = "Firma klasörü bulunamadı." });
				}

				// Silinecek dosyayı bul
				string filePattern = documentType == "registration" 
					? $"{company.RegistrationNumber}_ImzalıYetkiBelgeDilekcesi.*"
					: $"{company.RegistrationNumber}_ITOYetkiBelgesi.*";

				var possibleExtensions = new[] { "pdf", "jpg", "jpeg", "png", "doc", "docx" };
				string deletedFileName = null;

				foreach (var ext in possibleExtensions)
				{
					string fileName = documentType == "registration"
						? $"{company.RegistrationNumber}_ImzalıYetkiBelgeDilekcesi.{ext}"
						: $"{company.RegistrationNumber}_ITOYetkiBelgesi.{ext}";
					
					string filePath = Path.Combine(companyFolderPath, fileName);
					
					if (System.IO.File.Exists(filePath))
					{
						System.IO.File.Delete(filePath);
						deletedFileName = fileName;
						break;
					}
				}

				if (deletedFileName == null)
				{
					return NotFound(new { success = false, message = "Silinecek belge bulunamadı." });
				}

				// Log
				string documentName = documentType == "registration" ? "İmzalı Yetki Belgesi" : "Onaylı Yetki Belgesi";
				Console.WriteLine($"Belge silindi - Firma: {company.RegistrationNumber}, Belge: {documentName}, Dosya: {deletedFileName}");

				return Ok(new { 
					success = true, 
					message = $"{documentName} başarıyla silindi.",
					deletedFile = deletedFileName
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = $"Belge silme hatası: {ex.Message}" });
			}
		}
	}
}
