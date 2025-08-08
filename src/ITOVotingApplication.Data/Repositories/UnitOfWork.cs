using ITOVotingApplication.Core.Entities;
using ITOVotingApplication.Core.Interfaces;
using ITOVotingApplication.Data.Context;

namespace ITOVotingApplication.Data.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly VotingDbContext _context;
		private IGenericRepository<Company> _companies;
		private IGenericRepository<CompanyType> _companyTypes;
		private IGenericRepository<NaceCode> _naceCodes;
		private IGenericRepository<Contact> _contacts;
		private IGenericRepository<Committee> _committees;
		private IGenericRepository<User> _users;
		private IGenericRepository<Role> _roles;
		private IGenericRepository<UserRole> _userRoles;
		private IGenericRepository<BallotBox> _ballotBoxes;
		private IGenericRepository<VoteTransaction> _voteTransactions;

		public UnitOfWork(VotingDbContext context)
		{
			_context = context;
		}

		public IGenericRepository<Company> Companies =>
			_companies ??= new GenericRepository<Company>(_context);

		public IGenericRepository<CompanyType> CompanyTypes =>
			_companyTypes ??= new GenericRepository<CompanyType>(_context);

		public IGenericRepository<NaceCode> NaceCodes =>
			_naceCodes ??= new GenericRepository<NaceCode>(_context);

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