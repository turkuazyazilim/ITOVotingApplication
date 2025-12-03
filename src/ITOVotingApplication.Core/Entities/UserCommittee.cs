namespace ITOVotingApplication.Core.Entities
{
	public class UserCommittee : BaseEntity
	{
		public int UserId { get; set; }
		public int CommitteeId { get; set; }

		// Navigation Properties
		public virtual User User { get; set; }
		public virtual Committee Committee { get; set; }
	}
}
