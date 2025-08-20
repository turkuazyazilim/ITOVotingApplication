namespace ITOVotingApplication.Core.DTOs.User
{
	public class UserDto : BaseDto
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string FullName { get; set; }
		public bool IsActive { get; set; }
		public List<string> Roles { get; set; }
	}

	public class CreateUserDto
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public List<int> RoleIds { get; set; }
	}

	public class UpdateUserDto
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public bool IsActive { get; set; }
		public List<int> RoleIds { get; set; }
	}
}