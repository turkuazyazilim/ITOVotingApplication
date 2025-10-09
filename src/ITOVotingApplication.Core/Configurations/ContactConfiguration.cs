using ITOVotingApplication.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Data.Configurations
{
	public class ContactConfiguration : IEntityTypeConfiguration<Contact>
	{
		public void Configure(EntityTypeBuilder<Contact> builder)
		{
			builder.ToTable("cdContacts");

			builder.HasKey(e => e.Id);

			builder.Property(e => e.FirstName)
				.HasMaxLength(50)
				.IsRequired();

			builder.Property(e => e.LastName)
				.HasMaxLength(50)
				.IsRequired();

			builder.Property(e => e.MobilePhone)
				.HasMaxLength(20);

			builder.Property(e => e.Email)
				.HasMaxLength(100);

			builder.Property(e => e.IdentityNum)
				.HasMaxLength(11);

			// Relationships
			builder.HasOne(d => d.Company)
				.WithMany(p => p.Contacts)
				.HasForeignKey(d => d.CompanyId)
				.OnDelete(DeleteBehavior.Restrict);

			// Index
			builder.HasIndex(e => new { e.CompanyId, e.IdentityNum });
		}
	}
}