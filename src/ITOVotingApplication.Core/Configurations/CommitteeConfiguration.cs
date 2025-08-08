using ITOVotingApplication.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Data.Configurations
{
	public class CommitteeConfiguration : IEntityTypeConfiguration<Committee>
	{
		public void Configure(EntityTypeBuilder<Committee> builder)
		{
			builder.ToTable("cdCommittee");

			builder.HasKey(e => e.Id);

			builder.Property(e => e.CommitteeDescription)
				.HasMaxLength(100)
				.IsRequired();
		}
	}
}
