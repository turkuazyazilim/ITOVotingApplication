namespace ITOVotingApplication.Core.Entities
{
	public class BallotBox : BaseEntity
	{
		public string BallotBoxDescription { get; set; }

		// Navigation Properties
		public virtual ICollection<VoteTransaction> VoteTransactions { get; set; }

		public BallotBox()
		{
			VoteTransactions = new HashSet<VoteTransaction>();
		}
	}
}
