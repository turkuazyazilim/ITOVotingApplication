using AutoMapper;
using ITOVotingApplication.Core.DTOs.Committee;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Business.Services.Interfaces;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Business.Services
{
	public class CommitteeService : ICommitteeService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public CommitteeService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<ApiResponse<PagedResult<CommitteeDto>>> GetAllAsync(PagedRequest request)
		{
			try
			{
				var query = _unitOfWork.Committees.Query();

				// Search
				if (!string.IsNullOrWhiteSpace(request.SearchTerm))
				{
					query = query.Where(c => c.CommitteeDescription.Contains(request.SearchTerm));
				}

				// Sorting
				query = request.SortBy?.ToLower() switch
				{
					"description" => request.IsDescending ?
						query.OrderByDescending(c => c.CommitteeDescription) :
						query.OrderBy(c => c.CommitteeDescription),
					_ => query.OrderBy(c => c.Id)
				};

				var totalCount = await query.CountAsync();

				var committees = await query
					.Skip((request.PageNumber - 1) * request.PageSize)
					.Take(request.PageSize)
					.ToListAsync();

				var committeeDtos = _mapper.Map<List<CommitteeDto>>(committees);


				var result = new PagedResult<CommitteeDto>
				{
					Items = committeeDtos,
					TotalCount = totalCount,
					PageNumber = request.PageNumber,
					PageSize = request.PageSize
				};

				return ApiResponse<PagedResult<CommitteeDto>>.SuccessResult(result);
			}
			catch (Exception ex)
			{
				return ApiResponse<PagedResult<CommitteeDto>>.ErrorResult($"Komite listesi getirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<CommitteeDto>> GetByIdAsync(int id)
		{
			try
			{
				var committee = await _unitOfWork.Committees.GetByIdAsync(id);

				if (committee == null)
				{
					return ApiResponse<CommitteeDto>.ErrorResult("Komite bulunamadı.");
				}

				var dto = _mapper.Map<CommitteeDto>(committee);

				return ApiResponse<CommitteeDto>.SuccessResult(dto);
			}
			catch (Exception ex)
			{
				return ApiResponse<CommitteeDto>.ErrorResult($"Komite getirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<CommitteeDto>> CreateAsync(CreateCommitteeDto dto)
		{
			try
			{
				// Check for duplicate committee description
				var existingCommittee = await _unitOfWork.Committees
					.SingleOrDefaultAsync(c => c.CommitteeDescription == dto.CommitteeDescription);

				if (existingCommittee != null)
				{
					return ApiResponse<CommitteeDto>.ErrorResult("Bu isimde bir komite zaten mevcut.");
				}

				var committee = _mapper.Map<Committee>(dto);
				await _unitOfWork.Committees.AddAsync(committee);
				await _unitOfWork.CompleteAsync();

				var result = _mapper.Map<CommitteeDto>(committee);
				return ApiResponse<CommitteeDto>.SuccessResult(result, "Komite başarıyla oluşturuldu.");
			}
			catch (Exception ex)
			{
				return ApiResponse<CommitteeDto>.ErrorResult($"Komite oluşturma hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<CommitteeDto>> UpdateAsync(UpdateCommitteeDto dto)
		{
			try
			{
				var committee = await _unitOfWork.Committees.GetByIdAsync(dto.Id);

				if (committee == null)
				{
					return ApiResponse<CommitteeDto>.ErrorResult("Komite bulunamadı.");
				}

				// Check for duplicate committee description
				var existingCommittee = await _unitOfWork.Committees
					.SingleOrDefaultAsync(c => c.CommitteeDescription == dto.CommitteeDescription && c.Id != dto.Id);

				if (existingCommittee != null)
				{
					return ApiResponse<CommitteeDto>.ErrorResult("Bu isimde başka bir komite zaten mevcut.");
				}

				_mapper.Map(dto, committee);
				_unitOfWork.Committees.Update(committee);
				await _unitOfWork.CompleteAsync();

				var result = _mapper.Map<CommitteeDto>(committee);

				return ApiResponse<CommitteeDto>.SuccessResult(result, "Komite başarıyla güncellendi.");
			}
			catch (Exception ex)
			{
				return ApiResponse<CommitteeDto>.ErrorResult($"Komite güncelleme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<bool>> DeleteAsync(int id)
		{
			try
			{
				var committee = await _unitOfWork.Committees.GetByIdAsync(id);

				if (committee == null)
				{
					return ApiResponse<bool>.ErrorResult("Komite bulunamadı.");
				}



				_unitOfWork.Committees.Remove(committee);
				await _unitOfWork.CompleteAsync();

				return ApiResponse<bool>.SuccessResult(true, "Komite başarıyla silindi.");
			}
			catch (Exception ex)
			{
				return ApiResponse<bool>.ErrorResult($"Komite silme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<List<CommitteeDto>>> GetAllForDropdownAsync()
		{
			try
			{
				var committees = await _unitOfWork.Committees.Query()
					.OrderBy(c => c.CommitteeDescription)
					.ToListAsync();

				var committeeDtos = _mapper.Map<List<CommitteeDto>>(committees);

				return ApiResponse<List<CommitteeDto>>.SuccessResult(committeeDtos);
			}
			catch (Exception ex)
			{
				return ApiResponse<List<CommitteeDto>>.ErrorResult($"Komite listesi getirme hatası: {ex.Message}");
			}
		}
	}
}