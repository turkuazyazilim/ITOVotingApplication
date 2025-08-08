namespace ITOVotingApplication.Core.DTOs.Contact
{
	public class ContactDto : BaseDto
	{
		public int CompanyId { get; set; }
		public string CompanyTitle { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string FullName { get; set; }
		public int AuthorizationType { get; set; }
		public string AuthorizationTypeDescription { get; set; }
		public int? CommitteeId { get; set; }
		public string CommitteeDescription { get; set; }
		public string MobilePhone { get; set; }
		public string Email { get; set; }
		public string IdentityNum { get; set; }
		public bool EligibleToVote { get; set; }
		public bool HasVoted { get; set; }
	}

	public class CreateContactDto
	{
		public int CompanyId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int AuthorizationType { get; set; }
		public int? CommitteeId { get; set; }
		public string MobilePhone { get; set; }
		public string Email { get; set; }
		public string IdentityNum { get; set; }
		public bool EligibleToVote { get; set; }
	}

	public class UpdateContactDto : CreateContactDto
	{
		public int Id { get; set; }
	}
}