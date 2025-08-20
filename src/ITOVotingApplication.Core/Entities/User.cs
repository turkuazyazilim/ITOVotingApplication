namespace ITOVotingApplication.Core.Entities
{
	public class User : BaseEntity
	{
		public string UserName { get; set; }
		public string PasswordHash { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public bool IsActive { get; set; }

		// Navigation Properties
		public virtual ICollection<UserRole> UserRoles { get; set; }
		public virtual ICollection<VoteTransaction> CreatedVoteTransactions { get; set; }

		public User()
		{
			UserRoles = new HashSet<UserRole>();
			CreatedVoteTransactions = new HashSet<VoteTransaction>();
		}

		public string FullName => $"{FirstName} {LastName}";
	}
}
