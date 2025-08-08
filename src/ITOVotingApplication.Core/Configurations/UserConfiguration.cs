using ITOVotingApplication.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Data.Configurations
{
	public class UserConfiguration : IEntityTypeConfiguration<User>
	{
		public void Configure(EntityTypeBuilder<User> builder)
		{
			builder.ToTable("cdUsers");

			builder.HasKey(e => e.Id);

			builder.Property(e => e.UserName)
				.HasMaxLength(50)
				.IsRequired();

			builder.Property(e => e.PasswordHash)
				.HasMaxLength(256)
				.IsRequired();

			builder.Property(e => e.Email)
				.HasMaxLength(100)
				.IsRequired();

			builder.Property(e => e.FirstName)
				.HasMaxLength(50)
				.IsRequired();

			builder.Property(e => e.LastName)
				.HasMaxLength(50)
				.IsRequired();

			// Index
			builder.HasIndex(e => e.UserName).IsUnique();
			builder.HasIndex(e => e.Email).IsUnique();
		}
	}
}