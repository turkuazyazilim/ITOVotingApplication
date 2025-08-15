using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.Contact;

namespace ITOVotingApplication.Business.Interfaces
{
	public interface IContactService
	{
		Task<ApiResponse<ContactDto>> GetByIdAsync(int id);
		Task<ApiResponse<PagedResult<ContactDto>>> GetAllAsync(PagedRequest request);
		Task<ApiResponse<ContactDto>> CreateAsync(CreateContactDto dto);
		Task<ApiResponse<ContactDto>> UpdateAsync(UpdateContactDto dto);
		Task<ApiResponse<bool>> DeleteAsync(int id);
		Task<ApiResponse<List<ContactDto>>> GetByCompanyIdAsync(int companyId);
		Task<ApiResponse<List<ContactDto>>> GetEligibleVotersAsync(int ballotBoxId);
		Task<ApiResponse<ContactDto>> GetByIdentityNumAsync(string identityNum);
		Task<ApiResponse<int>> GetCountAsync(bool onlyEligible = false);
	}
}