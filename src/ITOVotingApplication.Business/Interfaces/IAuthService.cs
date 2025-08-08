using ITOVotingApplication.Core.DTOs.Auth;
using ITOVotingApplication.Core.DTOs.Common;
using ITOVotingApplication.Core.DTOs.User;

namespace ITOVotingApplication.Business.Interfaces
{
	public interface IAuthService
	{
		Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);
		Task<ApiResponse<UserDto>> RegisterAsync(RegisterDto registerDto);
		Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
		Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken);
		Task<ApiResponse<bool>> LogoutAsync(int userId);
	}
}