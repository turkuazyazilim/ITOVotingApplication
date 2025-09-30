using ITOVotingApplication.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITOVotingApplication.Data.Configurations
{
	public class FieldReferenceSubCategoryConfiguration : IEntityTypeConfiguration<FieldReferenceSubCategory>
	{
		public void Configure(EntityTypeBuilder<FieldReferenceSubCategory> builder)
		{
			builder.ToTable("prFieldReferenceSubCategories");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.CategoryId)
				.IsRequired();

			builder.Property(x => x.SubCategoryName)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(x => x.Description)
				.HasMaxLength(500);

			builder.Property(x => x.IsActive)
				.IsRequired()
				.HasDefaultValue(true);

			// Index for subcategory name uniqueness within the same category
			builder.HasIndex(x => new { x.CategoryId, x.SubCategoryName })
				.IsUnique()
				.HasDatabaseName("IX_FieldReferenceSubCategory_CategoryId_SubCategoryName");

			// Many-to-one relationship with Category
			builder.HasOne(x => x.Category)
				.WithMany(x => x.SubCategories)
				.HasForeignKey(x => x.CategoryId)
				.OnDelete(DeleteBehavior.Cascade);

			// One-to-many relationship with UserRoles
			builder.HasMany(x => x.UserRoles)
				.WithOne(x => x.FieldReferenceSubCategory)
				.HasForeignKey(x => x.FieldReferenceSubCategoryId)
				.OnDelete(DeleteBehavior.NoAction);
		}
	}
}