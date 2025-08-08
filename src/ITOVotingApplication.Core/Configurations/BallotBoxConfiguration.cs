using ITOVotingApplication.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITOVotingApplication.Data.Configurations
{
	public class BallotBoxConfiguration : IEntityTypeConfiguration<BallotBox>
	{
		public void Configure(EntityTypeBuilder<BallotBox> builder)
		{
			builder.ToTable("cdBallotBox");

			builder.HasKey(e => e.Id);

			builder.Property(e => e.BallotBoxDescription)
				.HasMaxLength(100)
				.IsRequired();
		}
	}
}