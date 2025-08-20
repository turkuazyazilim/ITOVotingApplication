namespace ITOVotingApplication.Core.Entities
{
	public class Company : BaseEntity
	{
		public string RegistrationNumber { get; set; }
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
		public int? ActiveContactId { get; set; }
		public bool IsActive { get; set; }
		public bool Has2022AuthorizationCertificate { get; set; }

		// Navigation Properties
		public virtual CompanyType CompanyTypeNavigation { get; set; }
		public virtual NaceCode NaceCodeNavigation { get; set; }
		public virtual Contact ActiveContact { get; set; }
		public virtual ICollection<Contact> Contacts { get; set; }
		public virtual ICollection<VoteTransaction> VoteTransactions { get; set; }

		public Company()
		{
			Contacts = new HashSet<Contact>();
			VoteTransactions = new HashSet<VoteTransaction>();
		}
	}
}