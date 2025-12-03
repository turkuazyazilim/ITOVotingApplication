// ITOVotingApplication.Data/Configurations/CompanyConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ITOVotingApplication.Core.Entities;

namespace ITOVotingApplication.Data.Configurations
{
	public class CompanyConfiguration : IEntityTypeConfiguration<Company>
	{
		public void Configure(EntityTypeBuilder<Company> builder)
		{
			builder.ToTable("cdCompany");

			builder.HasKey(e => e.Id);

			builder.Property(e => e.RegistrationNumber)
				.HasMaxLength(50)
				.IsRequired();

			builder.Property(e => e.Title)
				.HasMaxLength(250)
				.IsRequired();

			builder.Property(e => e.TradeRegistrationNumber)
				.HasMaxLength(50);

			builder.Property(e => e.RegistrationAddress)
				.HasMaxLength(500);

			builder.Property(e => e.ProfessionalGroup)
				.HasMaxLength(100);

			builder.Property(e => e.OfficePhone)
				.HasMaxLength(20);

			builder.Property(e => e.MobilePhone)
				.HasMaxLength(20);

			builder.Property(e => e.Email)
				.HasMaxLength(100);

			builder.Property(e => e.CompanyType)
				.HasMaxLength(100);

			builder.Property(e => e.DocumentStatus)
				.IsRequired();

			builder.Property(e => e.IsActive);

			builder.Property(e => e.Has2022AuthorizationCertificate);

			// Relationships
			builder.HasOne(d => d.ActiveContact)
				.WithMany(p => p.ActiveForCompanies)
				.HasForeignKey(d => d.ActiveContactId);

			builder.HasOne(d => d.Committee)
				.WithMany(p => p.Companies)
				.HasForeignKey(d => d.CommitteeId)
				.OnDelete(DeleteBehavior.SetNull);

			// Index
			builder.HasIndex(e => e.RegistrationNumber).IsUnique();
		}
	}
}