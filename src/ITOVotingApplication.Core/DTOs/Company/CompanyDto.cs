namespace ITOVotingApplication.Core.DTOs.Company
{
	public class CompanyDto : BaseDto
	{
		public string RegistrationNumber { get; set; }
		public string TaxNumber { get; set; }
		public string Title { get; set; }
		public string CompanyType { get; set; }
		public string CompanyTypeDescription { get; set; }
		public string TradeRegistrationNumber { get; set; }
		public decimal Capital { get; set; }
		public string RegistrationAddress { get; set; }
		public string Degree { get; set; }
		public DateTime MemberRegistrationDate { get; set; }
		public string ProfessionalGroup { get; set; }
		public string NaceCode { get; set; }
		public string NaceDescription { get; set; }
		public string OfficePhone { get; set; }
		public string MobilePhone { get; set; }
		public string Email { get; set; }
		public string WebSite { get; set; }
		public int? ActiveContactId { get; set; }
		public string ActiveContactName { get; set; }
		public bool IsActive { get; set; }
	}

	public class CreateCompanyDto
	{
		public string RegistrationNumber { get; set; }
		public string TaxNumber { get; set; }
		public string Title { get; set; }
		public string CompanyType { get; set; }
		public string TradeRegistrationNumber { get; set; }
		public decimal Capital { get; set; }
		public string RegistrationAddress { get; set; }
		public string Degree { get; set; }
		public DateTime MemberRegistrationDate { get; set; }
		public string ProfessionalGroup { get; set; }
		public string NaceCode { get; set; }
		public string OfficePhone { get; set; }
		public string MobilePhone { get; set; }
		public string Email { get; set; }
		public string WebSite { get; set; }
	}

	public class UpdateCompanyDto : CreateCompanyDto
	{
		public int Id { get; set; }
		public int? ActiveContactId { get; set; }
		public bool IsActive { get; set; }
	}
}