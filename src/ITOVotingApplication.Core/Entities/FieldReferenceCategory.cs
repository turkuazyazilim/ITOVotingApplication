namespace ITOVotingApplication.Core.Entities
{
	public class FieldReferenceCategory : BaseEntity
	{
		public string CategoryName { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; }

		// Navigation Properties
		public virtual ICollection<FieldReferenceSubCategory> SubCategories { get; set; }

		public FieldReferenceCategory()
		{
			SubCategories = new HashSet<FieldReferenceSubCategory>();
		}
	}
}