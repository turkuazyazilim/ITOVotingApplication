using ITOVotingApplication.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITOVotingApplication.Data.Configurations
{
	public class FieldReferenceCategoryConfiguration : IEntityTypeConfiguration<FieldReferenceCategory>
	{
		public void Configure(EntityTypeBuilder<FieldReferenceCategory> builder)
		{
			builder.ToTable("prFieldReferenceCategories");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.CategoryName)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(x => x.Description)
				.HasMaxLength(500);

			builder.Property(x => x.IsActive)
				.IsRequired()
				.HasDefaultValue(true);

			// Index for category name uniqueness
			builder.HasIndex(x => x.CategoryName)
				.IsUnique()
				.HasDatabaseName("IX_FieldReferenceCategory_CategoryName");

			// One-to-many relationship with SubCategories
			builder.HasMany(x => x.SubCategories)
				.WithOne(x => x.Category)
				.HasForeignKey(x => x.CategoryId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}