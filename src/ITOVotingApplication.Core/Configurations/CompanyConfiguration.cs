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

			builder.Property(e => e.TaxNumber)
				.HasMaxLength(20)
				.IsRequired();

			builder.Property(e => e.Title)
				.HasMaxLength(250)
				.IsRequired();

			builder.Property(e => e.CompanyType)
				.HasMaxLength(10)
				.IsRequired();

			builder.Property(e => e.TradeRegistrationNumber)
				.HasMaxLength(50);

			builder.Property(e => e.Capital)
				.HasColumnType("decimal(18,2)");

			builder.Property(e => e.RegistrationAddress)
				.HasMaxLength(500);

			builder.Property(e => e.Degree)
				.HasMaxLength(50);

			builder.Property(e => e.ProfessionalGroup)
				.HasMaxLength(100);

			builder.Property(e => e.NaceCode)
				.HasMaxLength(10);

			builder.Property(e => e.OfficePhone)
				.HasMaxLength(20);

			builder.Property(e => e.MobilePhone)
				.HasMaxLength(20);

			builder.Property(e => e.Email)
				.HasMaxLength(100);

			builder.Property(e => e.WebSite)
				.HasMaxLength(100);

			// Relationships
			builder.HasOne(d => d.CompanyTypeNavigation)
				.WithMany(p => p.Companies)
				.HasForeignKey(d => d.CompanyType)
				.HasPrincipalKey(p => p.CompanyTypeCode);

			builder.HasOne(d => d.NaceCodeNavigation)
				.WithMany(p => p.Companies)
				.HasForeignKey(d => d.NaceCode)
				.HasPrincipalKey(p => p.Code);

			builder.HasOne(d => d.ActiveContact)
				.WithMany(p => p.ActiveForCompanies)
				.HasForeignKey(d => d.ActiveContactId);

			// Index
			builder.HasIndex(e => e.RegistrationNumber).IsUnique();
			builder.HasIndex(e => e.TaxNumber).IsUnique();
		}
	}
}