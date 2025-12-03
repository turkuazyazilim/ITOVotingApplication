using ITOVotingApplication.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ITOVotingApplication.Data.Configurations
{
	public class UserCommitteeConfiguration : IEntityTypeConfiguration<UserCommittee>
	{
		public void Configure(EntityTypeBuilder<UserCommittee> builder)
		{
			builder.ToTable("prUserCommittee");

			builder.HasKey(e => e.Id);

			// Relationships
			builder.HasOne(d => d.User)
				.WithMany(p => p.UserCommittees)
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(d => d.Committee)
				.WithMany(p => p.UserCommittees)
				.HasForeignKey(d => d.CommitteeId)
				.OnDelete(DeleteBehavior.Restrict);

			// Index - User ve Committee kombinasyonu unique olmalı
			builder.HasIndex(e => new { e.UserId, e.CommitteeId }).IsUnique();
		}
	}
}
