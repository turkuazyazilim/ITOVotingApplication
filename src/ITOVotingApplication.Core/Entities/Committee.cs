namespace ITOVotingApplication.Core.Entities
{
	public class Committee : BaseEntity
	{
		public string CommitteeDescription { get; set; }

		// Navigation Properties
		public virtual ICollection<Contact> Contacts { get; set; }

		public Committee()
		{
			Contacts = new HashSet<Contact>();
		}
	}
}