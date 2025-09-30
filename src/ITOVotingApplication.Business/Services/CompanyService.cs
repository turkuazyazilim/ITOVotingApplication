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
		public async Task<ApiResponse<PagedResult<CompanyDto>>> GetAllAsync(PagedRequest request)
		{
			try
			{
				var query = _unitOfWork.Companies.Query()
					.Include(c => c.ActiveContact)
					.Include(c => c.Committee)
					.AsQueryable();

				// Search filter
				if (!string.IsNullOrWhiteSpace(request.SearchTerm))
				{
					query = query.Where(c =>
						c.Title.Contains(request.SearchTerm) ||
						c.RegistrationNumber.Contains(request.SearchTerm) ||
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

		public async Task<ApiResponse<CompanyDto>> GetByIdAsync(int id)
		{
			try
			{
				var company = await _unitOfWork.Companies.Query()
					.Include(c => c.ActiveContact)
					.Include(c => c.Committee)
					.FirstOrDefaultAsync(c => c.Id == id);

				if (company == null)
				{
					return ApiResponse<CompanyDto>.ErrorResult("Firma bulunamadı");
				}

				var companyDto = _mapper.Map<CompanyDto>(company);
				return ApiResponse<CompanyDto>.SuccessResult(companyDto);
			}
			catch (Exception ex)
			{
				return ApiResponse<CompanyDto>.ErrorResult($"Firma getirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<CompanyDto>> UpdateAsync(UpdateCompanyDto dto)
		{
			try
			{
				var company = await _unitOfWork.Companies.GetByIdAsync(dto.Id);
				if (company == null)
				{
					return ApiResponse<CompanyDto>.ErrorResult("Firma bulunamadı");
				}

				// Update only the allowed fields
				company.Title = dto.Title;
				company.TradeRegistrationNumber = dto.TradeRegistrationNumber;
				company.RegistrationAddress = dto.RegistrationAddress;
				company.IsActive = dto.IsActive;
				company.Has2022AuthorizationCertificate = dto.Has2022AuthorizationCertificate;

				_unitOfWork.Companies.Update(company);
				await _unitOfWork.CompleteAsync();

				var updatedCompanyDto = _mapper.Map<CompanyDto>(company);
				return ApiResponse<CompanyDto>.SuccessResult(updatedCompanyDto);
			}
			catch (Exception ex)
			{
				return ApiResponse<CompanyDto>.ErrorResult($"Firma güncelleme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<int>> GetCountAsync(bool onlyActive = true)
		{
			try
			{
				var query = _unitOfWork.Companies.Query();

				query = query.Where(c => c.IsActive);

				var count = await query.CountAsync();

				return ApiResponse<int>.SuccessResult(count);
			}
			catch (Exception ex)
			{
				return ApiResponse<int>.ErrorResult($"Firma sayısı getirme hatası: {ex.Message}");
			}
		}
	}
}