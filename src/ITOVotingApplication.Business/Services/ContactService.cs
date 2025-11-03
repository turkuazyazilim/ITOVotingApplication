using AutoMapper;
using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Contact;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Business.Services
{
	public class ContactService : IContactService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public ContactService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}
		public async Task<ApiResponse<int>> GetCountAsync(bool onlyEligible = false)
		{
			try
			{
				var query = _unitOfWork.Contacts.Query();

				if (onlyEligible)
				{
					query = query.Where(c => c.EligibleToVote);
				}

				var count = await query.CountAsync();

				return ApiResponse<int>.SuccessResult(count);
			}
			catch (Exception ex)
			{
				return ApiResponse<int>.ErrorResult($"Yetkili sayısı getirme hatası: {ex.Message}");
			}
		}
		public async Task<ApiResponse<ContactDto>> GetByIdAsync(int id)
		{
			try
			{
				var contact = await _unitOfWork.Contacts.Query()
					.Include(c => c.Company)
					.FirstOrDefaultAsync(c => c.Id == id);

				if (contact == null)
				{
					return ApiResponse<ContactDto>.ErrorResult("Yetkili kişi bulunamadı.");
				}

				var result = _mapper.Map<ContactDto>(contact);

				// Check if voted
				result.HasVoted = await _unitOfWork.VoteTransactions
					.Query()
					.AnyAsync(v => v.ContactId == id);

				return ApiResponse<ContactDto>.SuccessResult(result);
			}
			catch (Exception ex)
			{
				return ApiResponse<ContactDto>.ErrorResult($"Yetkili kişi getirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<PagedResult<ContactDto>>> GetAllAsync(PagedRequest request)
		{
			try
			{
				var query = _unitOfWork.Contacts.Query()
					.Include(c => c.Company)
					.AsQueryable();

				// Search filter
				if (!string.IsNullOrWhiteSpace(request.SearchTerm))
				{
					query = query.Where(c =>
						c.FirstName.Contains(request.SearchTerm) ||
						c.LastName.Contains(request.SearchTerm) ||
						c.IdentityNum.Contains(request.SearchTerm) ||
						c.Email.Contains(request.SearchTerm) ||
						c.Company.Title.Contains(request.SearchTerm));
				}

				// Sorting
				if (!string.IsNullOrWhiteSpace(request.SortBy))
				{
					query = request.SortBy.ToLower() switch
					{
						"name" => request.IsDescending ?
							query.OrderByDescending(c => c.FirstName).ThenByDescending(c => c.LastName) :
							query.OrderBy(c => c.FirstName).ThenBy(c => c.LastName),
						"company" => request.IsDescending ?
							query.OrderByDescending(c => c.Company.Title) :
							query.OrderBy(c => c.Company.Title),
						_ => query.OrderBy(c => c.Id)
					};
				}
				else
				{
					query = query.OrderBy(c => c.FirstName).ThenBy(c => c.LastName);
				}

				var totalCount = await query.CountAsync();

				var contacts = await query
					.Skip((request.PageNumber - 1) * request.PageSize)
					.Take(request.PageSize)
					.ToListAsync();

				var contactDtos = _mapper.Map<List<ContactDto>>(contacts);

				// Check voting status for each contact
				foreach (var dto in contactDtos)
				{
					dto.HasVoted = await _unitOfWork.VoteTransactions
						.Query()
						.AnyAsync(v => v.ContactId == dto.Id);
				}

				var result = new PagedResult<ContactDto>
				{
					Items = contactDtos,
					TotalCount = totalCount,
					PageNumber = request.PageNumber,
					PageSize = request.PageSize
				};

				return ApiResponse<PagedResult<ContactDto>>.SuccessResult(result);
			}
			catch (Exception ex)
			{
				return ApiResponse<PagedResult<ContactDto>>.ErrorResult($"Yetkili kişi listesi getirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<ContactDto>> CreateAsync(CreateContactDto dto)
		{
			try
			{
				// Validate company
				var company = await _unitOfWork.Companies.GetByIdAsync(dto.CompanyId);
				if (company == null)
				{
					return ApiResponse<ContactDto>.ErrorResult("Firma bulunamadı.");
				}

				// Check for duplicate identity number

				if (dto.IdentityNum != "" && dto.IdentityNum != "11111111111")
				{
					var existingContact = await _unitOfWork.Contacts
					.SingleOrDefaultAsync(c => c.IdentityNum == dto.IdentityNum);

					if (existingContact != null)
					{
						return ApiResponse<ContactDto>.ErrorResult("Bu TC kimlik numarası ile kayıtlı kişi bulunmaktadır.");
					}
				}

				var contact = _mapper.Map<Contact>(dto);
				await _unitOfWork.Contacts.AddAsync(contact);
				await _unitOfWork.CompleteAsync();

				var createdContact = await _unitOfWork.Contacts.Query()
					.Include(c => c.Company)
					.FirstOrDefaultAsync(c => c.Id == contact.Id);

				var result = _mapper.Map<ContactDto>(createdContact);
				return ApiResponse<ContactDto>.SuccessResult(result, "Yetkili kişi başarıyla oluşturuldu.");
			}
			catch (Exception ex)
			{
				return ApiResponse<ContactDto>.ErrorResult($"Yetkili kişi oluşturma hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<ContactDto>> UpdateAsync(UpdateContactDto dto)
		{
			try
			{
				var contact = await _unitOfWork.Contacts.GetByIdAsync(dto.Id);

				if (contact == null)
				{
					return ApiResponse<ContactDto>.ErrorResult("Yetkili kişi bulunamadı.");
				}

				// Check for duplicate identity number
				if (dto.IdentityNum != "11111111111")
				{
					var existingContact = await _unitOfWork.Contacts
					.SingleOrDefaultAsync(c => c.IdentityNum == dto.IdentityNum && c.Id != dto.Id);

					if (existingContact != null)
					{
						return ApiResponse<ContactDto>.ErrorResult("Bu TC kimlik numarası başka bir kişide kullanılmaktadır.");
					}
				}
				
				_mapper.Map(dto, contact);
				_unitOfWork.Contacts.Update(contact);
				await _unitOfWork.CompleteAsync();

				var updatedContact = await _unitOfWork.Contacts.Query()
					.Include(c => c.Company)
					.FirstOrDefaultAsync(c => c.Id == contact.Id);

				var result = _mapper.Map<ContactDto>(updatedContact);
				result.HasVoted = await _unitOfWork.VoteTransactions
					.Query()
					.AnyAsync(v => v.ContactId == dto.Id);

				return ApiResponse<ContactDto>.SuccessResult(result, "Yetkili kişi başarıyla güncellendi.");
			}
			catch (Exception ex)
			{
				return ApiResponse<ContactDto>.ErrorResult($"Yetkili kişi güncelleme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<bool>> DeleteAsync(int id)
		{
			try
			{
				var contact = await _unitOfWork.Contacts.GetByIdAsync(id);

				if (contact == null)
				{
					return ApiResponse<bool>.ErrorResult("Yetkili kişi bulunamadı.");
				}

				// Check if contact has voted
				var hasVoted = await _unitOfWork.VoteTransactions
					.Query()
					.AnyAsync(v => v.ContactId == id);

				if (hasVoted)
				{
					return ApiResponse<bool>.ErrorResult("Oy kullanan kişi silinemez.");
				}

				// Check if contact is active contact for any company
				var isActiveContact = await _unitOfWork.Companies
					.Query()
					.AnyAsync(c => c.ActiveContactId == id);

				if (isActiveContact)
				{
					return ApiResponse<bool>.ErrorResult("Firma yetkilisi olan kişi silinemez.");
				}

				_unitOfWork.Contacts.Remove(contact);
				await _unitOfWork.CompleteAsync();

				return ApiResponse<bool>.SuccessResult(true, "Yetkili kişi başarıyla silindi.");
			}
			catch (Exception ex)
			{
				return ApiResponse<bool>.ErrorResult($"Yetkili kişi silme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<List<ContactDto>>> GetByCompanyIdAsync(int companyId)
		{
			try
			{
				var contacts = await _unitOfWork.Contacts.Query()
					.Include(c => c.Company)
					.Where(c => c.CompanyId == companyId)
					.OrderBy(c => c.FirstName)
					.ThenBy(c => c.LastName)
					.ToListAsync();

				var contactDtos = _mapper.Map<List<ContactDto>>(contacts);

				// Check voting status for each contact
				foreach (var dto in contactDtos)
				{
					dto.HasVoted = await _unitOfWork.VoteTransactions
						.Query()
						.AnyAsync(v => v.ContactId == dto.Id);
				}

				return ApiResponse<List<ContactDto>>.SuccessResult(contactDtos);
			}
			catch (Exception ex)
			{
				return ApiResponse<List<ContactDto>>.ErrorResult($"Firma yetkilileri getirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<List<ContactDto>>> GetEligibleVotersAsync(int ballotBoxId)
		{
			try
			{
				var contacts = await _unitOfWork.Contacts.Query()
					.Include(c => c.Company)
					.Where(c => c.EligibleToVote)
					.ToListAsync();

				var contactDtos = _mapper.Map<List<ContactDto>>(contacts);

				// Check voting status for each contact in this ballot box
				foreach (var dto in contactDtos)
				{
					dto.HasVoted = await _unitOfWork.VoteTransactions
						.Query()
						.AnyAsync(v => v.ContactId == dto.Id && v.BallotBoxId == ballotBoxId);
				}

				// Filter only those who haven't voted in this ballot box
				var eligibleVoters = contactDtos.Where(c => !c.HasVoted).ToList();

				return ApiResponse<List<ContactDto>>.SuccessResult(eligibleVoters);
			}
			catch (Exception ex)
			{
				return ApiResponse<List<ContactDto>>.ErrorResult($"Oy kullanabilecek kişiler getirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<ContactDto>> GetByIdentityNumAsync(string identityNum)
		{
			try
			{
				var contact = await _unitOfWork.Contacts.Query()
					.Include(c => c.Company)
					.FirstOrDefaultAsync(c => c.IdentityNum == identityNum);

				if (contact == null)
				{
					return ApiResponse<ContactDto>.ErrorResult("Yetkili kişi bulunamadı.");
				}

				var result = _mapper.Map<ContactDto>(contact);
				result.HasVoted = await _unitOfWork.VoteTransactions
					.Query()
					.AnyAsync(v => v.ContactId == contact.Id);

				return ApiResponse<ContactDto>.SuccessResult(result);
			}
			catch (Exception ex)
			{
				return ApiResponse<ContactDto>.ErrorResult($"Yetkili kişi getirme hatası: {ex.Message}");
			}
		}

		public async Task<ApiResponse<ContactDto>> GetByCompanyRegistrationNumberAsync(string registrationNumber)
		{
			try
			{
				var contact = await _unitOfWork.Contacts.Query()
					.Include(c => c.Company)
					.Where(c => c.Company.RegistrationNumber == registrationNumber && c.EligibleToVote)
					.FirstOrDefaultAsync();

				if (contact == null)
				{
					return ApiResponse<ContactDto>.ErrorResult("Bu sicil numarasına ait oy kullanabilecek seçmen bulunamadı.");
				}

				var result = _mapper.Map<ContactDto>(contact);
				result.HasVoted = await _unitOfWork.VoteTransactions
					.Query()
					.AnyAsync(v => v.ContactId == contact.Id);

				return ApiResponse<ContactDto>.SuccessResult(result);
			}
			catch (Exception ex)
			{
				return ApiResponse<ContactDto>.ErrorResult($"Yetkili kişi getirme hatası: {ex.Message}");
			}
		}
	}
}