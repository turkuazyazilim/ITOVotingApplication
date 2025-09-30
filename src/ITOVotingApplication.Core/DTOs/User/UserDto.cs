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
		public int? FieldReferenceCategoryId { get; set; }
		public int? FieldReferenceSubCategoryId { get; set; }
		public string FieldReferenceCategoryName { get; set; }
		public string FieldReferenceSubCategoryName { get; set; }

		public UserDto()
		{
			Roles = new List<string>();
		}
	}

	public class CreateUserDto
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public bool IsActive { get; set; }
		public List<int> RoleIds { get; set; }
		public int? FieldReferenceCategoryId { get; set; }
		public int? FieldReferenceSubCategoryId { get; set; }

		public CreateUserDto()
		{
			RoleIds = new List<int>();
		}
	}

	public class UpdateUserDto
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public bool IsActive { get; set; }
		public List<int> RoleIds { get; set; }
		public int? FieldReferenceCategoryId { get; set; }
		public int? FieldReferenceSubCategoryId { get; set; }

		public UpdateUserDto()
		{
			RoleIds = new List<int>();
		}
	}
}