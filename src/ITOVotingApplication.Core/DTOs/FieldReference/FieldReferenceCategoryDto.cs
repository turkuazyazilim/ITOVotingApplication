namespace ITOVotingApplication.Core.DTOs.FieldReference
{
	public class FieldReferenceCategoryDto : BaseDto
	{
		public string CategoryName { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; }
		public int SubCategoryCount { get; set; }
		public List<FieldReferenceSubCategoryDto> SubCategories { get; set; }

		public FieldReferenceCategoryDto()
		{
			SubCategories = new List<FieldReferenceSubCategoryDto>();
		}
	}

	public class CreateFieldReferenceCategoryDto
	{
		public string CategoryName { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; } = true;
	}

	public class UpdateFieldReferenceCategoryDto
	{
		public int Id { get; set; }
		public string CategoryName { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; }
	}
}