using ITOVotingApplication.Core.DTOs.User;

namespace ITOVotingApplication.Core.DTOs.Auth
{
	public class LoginDto
	{
		public string UserName { get; set; }
		public string Password { get; set; }
	}

	public class LoginResponseDto
	{
		public string Token { get; set; }
		public string RefreshToken { get; set; }
		public DateTime ExpiresAt { get; set; }
		public UserDto User { get; set; }
	}

	public class RegisterDto
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}

	public class ChangePasswordDto
	{
		public string CurrentPassword { get; set; }
		public string NewPassword { get; set; }
		public string ConfirmPassword { get; set; }
	}
}