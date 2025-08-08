namespace ITOVotingApplication.Core.DTOs.Vote
{
	public class VoteDto : BaseDto
	{
		public int CompanyId { get; set; }
		public string CompanyTitle { get; set; }
		public int ContactId { get; set; }
		public string ContactName { get; set; }
		public int BallotBoxId { get; set; }
		public string BallotBoxDescription { get; set; }
		public string Description { get; set; }
		public int CreatedUserId { get; set; }
		public string CreatedUserName { get; set; }
		public DateTime CreatedDate { get; set; }
	}

	public class CastVoteDto
	{
		public int ContactId { get; set; }
		public int BallotBoxId { get; set; }
		public string Description { get; set; }
	}

	public class VoteResultDto
	{
		public int BallotBoxId { get; set; }
		public string BallotBoxDescription { get; set; }
		public int TotalVotes { get; set; }
		public int EligibleVoters { get; set; }
		public decimal TurnoutPercentage { get; set; }
		public DateTime LastVoteDate { get; set; }
	}
}