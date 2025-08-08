namespace ITOVotingApplication.Core.Entities
{
	public class CompanyType
	{
		public string CompanyTypeCode { get; set; }
		public string CompanyTypeDescription { get; set; }

		// Navigation Properties
		public virtual ICollection<Company> Companies { get; set; }

		public CompanyType()
		{
			Companies = new HashSet<Company>();
		}
	}
}