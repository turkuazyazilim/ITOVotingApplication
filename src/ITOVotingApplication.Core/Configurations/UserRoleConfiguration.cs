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

			// Relationships
			builder.HasOne(d => d.User)
				.WithMany(p => p.UserRoles)
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(d => d.Role)
				.WithMany(p => p.UserRoles)
				.HasForeignKey(d => d.RoleId)
				.OnDelete(DeleteBehavior.Restrict);

			// Index
			builder.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
		}
	}
}