namespace ITOVotingApplication.Core.Entities
{
	public class Role : BaseEntity
	{
		public string RoleDescription { get; set; }
		public bool IsActive { get; set; }

		// Navigation Properties
		public virtual ICollection<UserRole> UserRoles { get; set; }

		public Role()
		{
			UserRoles = new HashSet<UserRole>();
		}
	}
}