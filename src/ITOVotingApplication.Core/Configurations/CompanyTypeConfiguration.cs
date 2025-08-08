using ITOVotingApplication.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Data.Configurations
{
	public class CompanyTypeConfiguration : IEntityTypeConfiguration<CompanyType>
	{
		public void Configure(EntityTypeBuilder<CompanyType> builder)
		{
			builder.ToTable("cdCompanyTypes");

			builder.HasKey(e => e.CompanyTypeCode);

			builder.Property(e => e.CompanyTypeCode)
				.HasMaxLength(10);

			builder.Property(e => e.CompanyTypeDescription)
				.HasMaxLength(100)
				.IsRequired();
		}
	}
}