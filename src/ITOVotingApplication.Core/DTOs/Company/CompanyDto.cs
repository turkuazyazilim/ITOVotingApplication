namespace ITOVotingApplication.Core.DTOs.Company
{
	public class CompanyDto : BaseDto
	{
		public string RegistrationNumber { get; set; }
		public string Title { get; set; }
		public string TradeRegistrationNumber { get; set; }
		public string RegistrationAddress { get; set; }
		public string ProfessionalGroup { get; set; }
		public string OfficePhone { get; set; }
		public string MobilePhone { get; set; }
		public string Email { get; set; }
		public int? ActiveContactId { get; set; }
		public string ActiveContactName { get; set; }
		public int? CommitteeId { get; set; }
		public string CommitteeDescription { get; set; }
		public bool IsActive { get; set; }
		public bool Has2022AuthorizationCertificate { get; set; }
	}

	public class CreateCompanyDto
	{
		public string RegistrationNumber { get; set; }
		public string Title { get; set; }
		public string TradeRegistrationNumber { get; set; }
		public string RegistrationAddress { get; set; }
		public string ProfessionalGroup { get; set; }
		public string OfficePhone { get; set; }
		public string MobilePhone { get; set; }
		public string Email { get; set; }
		public int? CommitteeId { get; set; }
		public bool IsActive { get; set; }
		public bool Has2022AuthorizationCertificate { get; set; }
	}

	public class UpdateCompanyDto : CreateCompanyDto
	{
		public int Id { get; set; }
		public int? ActiveContactId { get; set; }
	}
}