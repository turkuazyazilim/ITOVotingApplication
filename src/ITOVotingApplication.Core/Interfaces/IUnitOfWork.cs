using ITOVotingApplication.Core.Entities;

namespace ITOVotingApplication.Core.Interfaces
{
	public interface IUnitOfWork : IDisposable
	{
		IGenericRepository<Company> Companies { get; }
		IGenericRepository<Contact> Contacts { get; }
		IGenericRepository<Committee> Committees { get; }
		IGenericRepository<User> Users { get; }
		IGenericRepository<Role> Roles { get; }
		IGenericRepository<UserRole> UserRoles { get; }
		IGenericRepository<BallotBox> BallotBoxes { get; }
		IGenericRepository<VoteTransaction> VoteTransactions { get; }
		IGenericRepository<FieldReferenceCategory> FieldReferenceCategories { get; }
		IGenericRepository<FieldReferenceSubCategory> FieldReferenceSubCategories { get; }
		IGenericRepository<UserInvitation> UserInvitations { get; }
		Task<int> CompleteAsync();
		Task<bool> SaveChangesAsync();
	}
}