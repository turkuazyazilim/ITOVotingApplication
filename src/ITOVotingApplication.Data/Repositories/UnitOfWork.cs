using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using ITOVotingApplication.Data.Context;

namespace ITOVotingApplication.Data.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly VotingDbContext _context;
		private IGenericRepository<Company> _companies;
		private IGenericRepository<Contact> _contacts;
		private IGenericRepository<Committee> _committees;
		private IGenericRepository<User> _users;
		private IGenericRepository<Role> _roles;
		private IGenericRepository<UserRole> _userRoles;
		private IGenericRepository<BallotBox> _ballotBoxes;
		private IGenericRepository<VoteTransaction> _voteTransactions;
		private IGenericRepository<FieldReferenceCategory> _fieldReferenceCategories;
		private IGenericRepository<FieldReferenceSubCategory> _fieldReferenceSubCategories;
		private IGenericRepository<UserInvitation> _userInvitations;
		private IGenericRepository<CompanyDocumentTransaction> _companyDocumentTransactions;

		public UnitOfWork(VotingDbContext context)
		{
			_context = context;
		}

		public IGenericRepository<Company> Companies =>
			_companies ??= new GenericRepository<Company>(_context);

		public IGenericRepository<Contact> Contacts =>
			_contacts ??= new GenericRepository<Contact>(_context);

		public IGenericRepository<Committee> Committees =>
			_committees ??= new GenericRepository<Committee>(_context);

		public IGenericRepository<User> Users =>
			_users ??= new GenericRepository<User>(_context);

		public IGenericRepository<Role> Roles =>
			_roles ??= new GenericRepository<Role>(_context);

		public IGenericRepository<UserRole> UserRoles =>
			_userRoles ??= new GenericRepository<UserRole>(_context);

		public IGenericRepository<BallotBox> BallotBoxes =>
			_ballotBoxes ??= new GenericRepository<BallotBox>(_context);

		public IGenericRepository<VoteTransaction> VoteTransactions =>
			_voteTransactions ??= new GenericRepository<VoteTransaction>(_context);

		public IGenericRepository<FieldReferenceCategory> FieldReferenceCategories =>
			_fieldReferenceCategories ??= new GenericRepository<FieldReferenceCategory>(_context);

		public IGenericRepository<FieldReferenceSubCategory> FieldReferenceSubCategories =>
			_fieldReferenceSubCategories ??= new GenericRepository<FieldReferenceSubCategory>(_context);

		public IGenericRepository<UserInvitation> UserInvitations =>
			_userInvitations ??= new GenericRepository<UserInvitation>(_context);

		public IGenericRepository<CompanyDocumentTransaction> CompanyDocumentTransactions =>
			_companyDocumentTransactions ??= new GenericRepository<CompanyDocumentTransaction>(_context);

		public async Task<int> CompleteAsync()
		{
			return await _context.SaveChangesAsync();
		}

		public async Task<bool> SaveChangesAsync()
		{
			return await _context.SaveChangesAsync() > 0;
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}