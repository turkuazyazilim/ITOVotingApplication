namespace ITOVotingApplication.Core.Entities
{
	public class Committee : BaseEntity
	{
		public string CommitteeNum { get; set; }
		public string CommitteeDescription { get; set; }

		// Navigation Properties
		public virtual ICollection<Company> Companies { get; set; }
		public virtual ICollection<UserCommittee> UserCommittees { get; set; }

		public Committee()
		{
			Companies = new HashSet<Company>();
			UserCommittees = new HashSet<UserCommittee>();
		}
	}
}