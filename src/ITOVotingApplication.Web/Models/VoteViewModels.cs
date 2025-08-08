using ITOVotingApplication.Core.DTOs.Contact;
using ITOVotingApplication.Core.DTOs.Vote;
using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Web.Models
{
	public class VoteDashboardViewModel
	{
		public string UserName { get; set; }
		public string FullName { get; set; }
		public List<ContactDto> EligibleVoters { get; set; }
		public VoteResultDto VoteResults { get; set; }
		public int BallotBoxId { get; set; }
	}

	public class CastVoteViewModel
	{
		public int ContactId { get; set; }

		[Display(Name = "Seçmen")]
		public string ContactName { get; set; }

		[Display(Name = "Firma")]
		public string CompanyName { get; set; }

		[Required(ErrorMessage = "Sandık seçimi zorunludur.")]
		[Display(Name = "Sandık")]
		public int BallotBoxId { get; set; }

		[Display(Name = "Açıklama")]
		[StringLength(500)]
		public string Description { get; set; }
	}

	public class VoteResultsViewModel
	{
		public List<VoteResultDto> Results { get; set; }
	}
}