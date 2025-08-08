using AutoMapper;
using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Company;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Business.Services
{
	public class CompanyService : ICompanyService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public CompanyService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<ApiResponse<CompanyDto>> GetByIdAsync(int id)
		{
			try
			{
				var company = await _unitOfWork.Companies.Query()
					.Include(c => c.CompanyTypeNavigation)
					.Include(c => c.NaceCodeNavigation)
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

		public async Task<ApiResponse<PagedResult<CompanyDto>>> GetAllAsync(PagedRequest request)
		{
			try
			{
				var query = _unitOfWork.Companies.Query()
					.Include(c => c.CompanyTypeNavigation)
					.Include(c => c.NaceCodeNavigation)
					.Include(c => c.ActiveContact)
					.AsQueryable();

				// Search filter
				if (!string.IsNullOrWhiteSpace(request.SearchTerm))
				{
					query = query.Where(c =>
						c.Title.Contains(request.SearchTerm) ||
						c.RegistrationNumber.Contains(request.SearchTerm) ||
						c.TaxNumber.Contains(request.SearchTerm) ||
						c.TradeRegistrationNumber.Contains(request.SearchTerm));
				}

				// Sorting
				if (!string.IsNullOrWhiteSpace(request.SortBy))
				{
					query = request.SortBy.ToLower() switch
					{
						"title" => request.IsDescending ?
							query.OrderByDescending(c => c.Title) :
							query.OrderBy(c => c.Title),
						"registrationdate" => request.IsDescending ?
							query.OrderByDescending(c => c.MemberRegistrationDate) :
							query.OrderBy(c => c.MemberRegistrationDate),
						"capital" => request.IsDescending ?
							query.OrderByDescending(c => c.Capital) :
							query.OrderBy(c => c.Capital),
						_ => query.OrderBy(c => c.Id)
					};
				}
				else
				{
					query = query.OrderBy(c => c.Title);
				}

				var totalCount = await query.CountAsync();

				var companies = await query
					.Skip((request.PageNumber - 1) * request.PageSize)
					.Take(request.PageSize)
					.ToListAsync();

				var result = new PagedResult<CompanyDto>
				{
					Items = _mapper.Map<List<CompanyDto>>(companies),
					TotalCount = totalCount,
					PageNumber = request.PageNumber,
					PageSize = request.PageSize
				};

				return ApiResponse<PagedResult<CompanyDto>>.SuccessResult(result);
			}
			catch (Exception ex)
			{
				return ApiResponse<PagedResult<CompanyDto>>.ErrorResult($"Firma listesi getirme hatası: {ex.Message}");
			}
		}

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

				// Check for duplicate tax number
				existingCompany = await _unitOfWork.Companies
					.SingleOrDefaultAsync(c => c.TaxNumber == dto.TaxNumber);

				if (existingCompany != null)
				{
					return ApiResponse<CompanyDto>.ErrorResult("Bu vergi numarası ile kayıtlı firma bulunmaktadır.");
				}

				// Validate company type
				var companyType = await _unitOfWork.CompanyTypes
					.SingleOrDefaultAsync(ct => ct.CompanyTypeCode == dto.CompanyType);

				if (companyType == null)
				{
					return ApiResponse<CompanyDto>.ErrorResult("Geçersiz şirket tipi.");
				}

				// Validate NACE code if provided
				if (!string.IsNullOrWhiteSpace(dto.NaceCode))
				{
					var naceCode = await _unitOfWork.NaceCodes
						.SingleOrDefaultAsync(n => n.Code == dto.NaceCode);

					if (naceCode == null)
					{
						return ApiResponse<CompanyDto>.ErrorResult("Geçersiz NACE kodu.");
					}
				}

				var company = _mapper.Map<Company>(dto);
				company.IsActive = true;

				await _unitOfWork.Companies.AddAsync(company);
				await _unitOfWork.CompleteAsync();

				var createdCompany = await _unitOfWork.Companies.Query()
					.Include(c => c.CompanyTypeNavigation)
					.Include(c => c.NaceCodeNavigation)
					.FirstOrDefaultAsync(c => c.Id == company.Id);

				var result = _mapper.Map<CompanyDto>(createdCompany);
				return ApiResponse<CompanyDto>.SuccessResult(result, "Firma başarıyla oluşturuldu.");
			}
			catch (Exception ex)
			{
				return ApiResponse<CompanyDto>.ErrorResult($"Firma oluşturma hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<CompanyDto>> UpdateAsync(UpdateCompanyDto dto)
		{
			try
			{
				var company = await _unitOfWork.Companies.GetByIdAsync(dto.Id);

				if (company == null)
				{
					return ApiResponse<CompanyDto>.ErrorResult("Firma bulunamadı.");
				}

				// Check for duplicate registration number
				var existingCompany = await _unitOfWork.Companies
					.SingleOrDefaultAsync(c => c.RegistrationNumber == dto.RegistrationNumber && c.Id != dto.Id);

				if (existingCompany != null)
				{
					return ApiResponse<CompanyDto>.ErrorResult("Bu sicil numarası başka bir firmada kullanılmaktadır.");
				}

				// Check for duplicate tax number
				existingCompany = await _unitOfWork.Companies
					.SingleOrDefaultAsync(c => c.TaxNumber == dto.TaxNumber && c.Id != dto.Id);

				if (existingCompany != null)
				{
					return ApiResponse<CompanyDto>.ErrorResult("Bu vergi numarası başka bir firmada kullanılmaktadır.");
				}

				// Validate active contact if provided
				if (dto.ActiveContactId.HasValue)
				{
					var contact = await _unitOfWork.Contacts
						.SingleOrDefaultAsync(c => c.Id == dto.ActiveContactId.Value && c.CompanyId == dto.Id);

					if (contact == null)
					{
						return ApiResponse<CompanyDto>.ErrorResult("Geçersiz yetkili kişi.");
					}
				}

				_mapper.Map(dto, company);
				_unitOfWork.Companies.Update(company);
				await _unitOfWork.CompleteAsync();

				var updatedCompany = await _unitOfWork.Companies.Query()
					.Include(c => c.CompanyTypeNavigation)
					.Include(c => c.NaceCodeNavigation)
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

		public async Task<ApiResponse<List<CompanyDto>>> GetActiveCompaniesAsync()
		{
			try
			{
				var companies = await _unitOfWork.Companies.Query()
					.Include(c => c.CompanyTypeNavigation)
					.Include(c => c.NaceCodeNavigation)
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

		public async Task<ApiResponse<CompanyDto>> GetByRegistrationNumberAsync(string registrationNumber)
		{
			try
			{
				var company = await _unitOfWork.Companies.Query()
					.Include(c => c.CompanyTypeNavigation)
					.Include(c => c.NaceCodeNavigation)
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
	}
}