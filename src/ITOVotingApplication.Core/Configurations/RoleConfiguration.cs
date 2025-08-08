using ITOVotingApplication.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Data.Configurations
{
	public class RoleConfiguration : IEntityTypeConfiguration<Role>
	{
		public void Configure(EntityTypeBuilder<Role> builder)
		{
			builder.ToTable("cdRoles");

			builder.HasKey(e => e.Id);

			builder.Property(e => e.RoleDescription)
				.HasMaxLength(100)
				.IsRequired();
		}
	}
}
