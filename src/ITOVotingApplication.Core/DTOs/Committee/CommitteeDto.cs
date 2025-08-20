// CommitteeDto.cs
namespace ITOVotingApplication.Core.DTOs.Committee
{
	public class CommitteeDto
	{
		public int Id { get; set; }
		public string CommitteeDescription { get; set; }
		public int ContactCount { get; set; } // Komiteye bağlı kişi sayısı
		public DateTime CreatedDate { get; set; }
		public DateTime? UpdatedDate { get; set; }
	}
	public class CreateCommitteeDto
	{
		public string CommitteeDescription { get; set; }
	}
	public class UpdateCommitteeDto
	{
		public int Id { get; set; }
		public string CommitteeDescription { get; set; }
	}
}