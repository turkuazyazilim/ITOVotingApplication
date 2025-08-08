namespace ITOVotingApplication.Core.Entities
{
	public class UserRole : BaseEntity
	{
		public int UserId { get; set; }
		public int RoleId { get; set; }
		public bool IsActive { get; set; }

		// Navigation Properties
		public virtual User User { get; set; }
		public virtual Role Role { get; set; }
	}
}