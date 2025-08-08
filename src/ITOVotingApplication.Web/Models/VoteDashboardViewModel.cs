using ITOVotingApplication.Core.DTOs.Contact;
using ITOVotingApplication.Core.DTOs.Vote;

namespace ITOVotingApplication.Web.Models
{
	public class VoteDashboardViewModel
	{
		public string UserName { get; set; }
		public string FullName { get; set; }
		public int BallotBoxId { get; set; }
		public List<ContactDto> EligibleVoters { get; set; }
		public VoteResultDto VoteResults { get; set; }
	}
}
