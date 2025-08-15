using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.User;

namespace ITOVotingApplication.Business.Interfaces
{
	public interface IUserService
	{
		Task<ApiResponse<UserDto>> GetByIdAsync(int id);
		Task<ApiResponse<PagedResult<UserDto>>> GetAllAsync(PagedRequest request);
		Task<ApiResponse<UserDto>> CreateAsync(CreateUserDto dto);
		Task<ApiResponse<UserDto>> UpdateAsync(UpdateUserDto dto);
		Task<ApiResponse<bool>> DeleteAsync(int id);
		Task<ApiResponse<UserDto>> GetByUserNameAsync(string userName);
		Task<ApiResponse<bool>> AssignRoleAsync(int userId, List<int> roleIds);
		Task<ApiResponse<List<string>>> GetUserRolesAsync(int userId);
		Task<ApiResponse<bool>> ValidateUserAsync(string userName, string password);
		Task<ApiResponse<int>> GetActiveUserCountAsync();
		Task<ApiResponse<int>> GetTotalUserCountAsync();
	}
}