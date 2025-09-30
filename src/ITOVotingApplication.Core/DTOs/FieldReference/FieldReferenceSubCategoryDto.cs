namespace ITOVotingApplication.Core.DTOs.FieldReference
{
	public class FieldReferenceSubCategoryDto : BaseDto
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; }
		public string SubCategoryName { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; }
	}

	public class CreateFieldReferenceSubCategoryDto
	{
		public int CategoryId { get; set; }
		public string SubCategoryName { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; } = true;
	}

	public class UpdateFieldReferenceSubCategoryDto
	{
		public int Id { get; set; }
		public int CategoryId { get; set; }
		public string SubCategoryName { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; }
	}
}