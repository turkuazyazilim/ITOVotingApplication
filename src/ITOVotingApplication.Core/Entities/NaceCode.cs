namespace ITOVotingApplication.Core.Entities
{
	public class NaceCode
	{
		public string Code { get; set; }
		public string NaceDescription { get; set; }

		// Navigation Properties
		public virtual ICollection<Company> Companies { get; set; }

		public NaceCode()
		{
			Companies = new HashSet<Company>();
		}
	}
}
