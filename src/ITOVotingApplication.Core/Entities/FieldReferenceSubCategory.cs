namespace ITOVotingApplication.Core.Entities
{
	public class FieldReferenceSubCategory : BaseEntity
	{
		public int CategoryId { get; set; }
		public string SubCategoryName { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; }

		// Navigation Properties
		public virtual FieldReferenceCategory Category { get; set; }
		public virtual ICollection<UserRole> UserRoles { get; set; }

		public FieldReferenceSubCategory()
		{
			UserRoles = new HashSet<UserRole>();
		}
	}
}