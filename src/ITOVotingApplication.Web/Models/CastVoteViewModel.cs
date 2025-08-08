using System.ComponentModel.DataAnnotations;

namespace ITOVotingApplication.Web.Models
{
	public class CastVoteViewModel
	{
		public int ContactId { get; set; }
		public string ContactName { get; set; }
		public string CompanyName { get; set; }
		public int BallotBoxId { get; set; }

		[Display(Name = "Açıklama")]
		[StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
		public string Description { get; set; }
	}
}
