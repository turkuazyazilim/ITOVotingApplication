// ITOVotingApplication.Data/Context/VotingDbContext.cs
using Microsoft.EntityFrameworkCore;
using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Data.Configurations;

namespace ITOVotingApplication.Data.Context
{
	public class VotingDbContext : DbContext
	{
		public VotingDbContext(DbContextOptions<VotingDbContext> options) : base(options)
		{
		}

		// DbSets
		public DbSet<Company> Companies { get; set; }
		public DbSet<CompanyType> CompanyTypes { get; set; }
		public DbSet<NaceCode> NaceCodes { get; set; }
		public DbSet<Contact> Contacts { get; set; }
		public DbSet<Committee> Committees { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<UserRole> UserRoles { get; set; }
		public DbSet<BallotBox> BallotBoxes { get; set; }
		public DbSet<VoteTransaction> VoteTransactions { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Apply configurations
			modelBuilder.ApplyConfiguration(new CompanyConfiguration());
			modelBuilder.ApplyConfiguration(new CompanyTypeConfiguration());
			modelBuilder.ApplyConfiguration(new NaceCodeConfiguration());
			modelBuilder.ApplyConfiguration(new ContactConfiguration());
			modelBuilder.ApplyConfiguration(new CommitteeConfiguration());
			modelBuilder.ApplyConfiguration(new UserConfiguration());
			modelBuilder.ApplyConfiguration(new RoleConfiguration());
			modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
			modelBuilder.ApplyConfiguration(new BallotBoxConfiguration());
			modelBuilder.ApplyConfiguration(new VoteTransactionConfiguration());
		}
	}
}