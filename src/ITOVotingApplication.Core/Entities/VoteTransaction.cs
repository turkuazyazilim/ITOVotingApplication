namespace ITOVotingApplication.Core.Entities
{
	public class VoteTransaction : BaseEntity
	{
		public int CompanyId { get; set; }
		public int ContactId { get; set; }
		public int BallotBoxId { get; set; }
		public string Description { get; set; }
		public int CreatedUserId { get; set; }
		public DateTime CreatedDate { get; set; }

		// Navigation Properties
		public virtual Company Company { get; set; }
		public virtual Contact Contact { get; set; }
		public virtual BallotBox BallotBox { get; set; }
		public virtual User CreatedUser { get; set; }
	}
}