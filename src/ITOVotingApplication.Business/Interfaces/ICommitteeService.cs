using ITOVotingApplication.Core.DTOs.Committee;
using ITOVotingApplication.Core.DTOs.Common;

namespace ITOVotingApplication.Business.Services.Interfaces
{
	public interface ICommitteeService
	{
		Task<ApiResponse<PagedResult<CommitteeDto>>> GetAllAsync(PagedRequest request);
		Task<ApiResponse<CommitteeDto>> GetByIdAsync(int id);
		Task<ApiResponse<CommitteeDto>> CreateAsync(CreateCommitteeDto dto);
		Task<ApiResponse<CommitteeDto>> UpdateAsync(UpdateCommitteeDto dto);
		Task<ApiResponse<bool>> DeleteAsync(int id);
		Task<ApiResponse<List<CommitteeDto>>> GetAllForDropdownAsync();
	}
}