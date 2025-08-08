using ITOVotingApplication.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Data.Configurations
{
	public class NaceCodeConfiguration : IEntityTypeConfiguration<NaceCode>
	{
		public void Configure(EntityTypeBuilder<NaceCode> builder)
		{
			builder.ToTable("cdNaceCodes");

			builder.HasKey(e => e.Code);

			builder.Property(e => e.Code)
				.HasColumnName("NaceCode")
				.HasMaxLength(10);

			builder.Property(e => e.NaceDescription)
				.HasMaxLength(250)
				.IsRequired();
		}
	}
}
