namespace ITOVotingApplication.Core.Entities
{
	public class UserRole : BaseEntity
	{
		public int UserId { get; set; }
		public int RoleId { get; set; }
		public bool IsActive { get; set; }

		// Field Reference Properties (nullable - only for Saha Görevlisi role)
		public int? FieldReferenceCategoryId { get; set; }
		public int? FieldReferenceSubCategoryId { get; set; }

		// Navigation Properties
		public virtual User User { get; set; }
		public virtual Role Role { get; set; }
		public virtual FieldReferenceCategory FieldReferenceCategory { get; set; }
		public virtual FieldReferenceSubCategory FieldReferenceSubCategory { get; set; }
	}
}