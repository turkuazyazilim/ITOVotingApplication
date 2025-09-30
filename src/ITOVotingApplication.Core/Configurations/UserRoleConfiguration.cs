using ITOVotingApplication.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Data.Configurations
{
	public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
	{
		public void Configure(EntityTypeBuilder<UserRole> builder)
		{
			builder.ToTable("prUsersRoles");

			builder.HasKey(e => e.Id);

			// Properties
			builder.Property(x => x.FieldReferenceCategoryId)
				.IsRequired(false);

			builder.Property(x => x.FieldReferenceSubCategoryId)
				.IsRequired(false);

			// Relationships
			builder.HasOne(d => d.User)
				.WithMany(p => p.UserRoles)
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(d => d.Role)
				.WithMany(p => p.UserRoles)
				.HasForeignKey(d => d.RoleId)
				.OnDelete(DeleteBehavior.Restrict);

			// Field Reference relationships (nullable)
			builder.HasOne(d => d.FieldReferenceCategory)
				.WithMany()
				.HasForeignKey(d => d.FieldReferenceCategoryId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.HasOne(d => d.FieldReferenceSubCategory)
				.WithMany(p => p.UserRoles)
				.HasForeignKey(d => d.FieldReferenceSubCategoryId)
				.OnDelete(DeleteBehavior.NoAction);

			// Index
			builder.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
		}
	}
}