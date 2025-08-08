namespace ITOVotingApplication.Core.Entities
{
	public class Contact : BaseEntity
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

		// Navigation Properties
		public virtual Company Company { get; set; }
		public virtual Committee Committee { get; set; }
		public virtual ICollection<Company> ActiveForCompanies { get; set; }
		public virtual ICollection<VoteTransaction> VoteTransactions { get; set; }

		public Contact()
		{
			ActiveForCompanies = new HashSet<Company>();
			VoteTransactions = new HashSet<VoteTransaction>();
		}

		public string FullName => $"{FirstName} {LastName}";
	}
}