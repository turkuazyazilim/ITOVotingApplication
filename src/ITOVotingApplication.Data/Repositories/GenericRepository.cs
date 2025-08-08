using ITOVotingApplication.Core.Interfaces;
using ITOVotingApplication.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ITOVotingApplication.Data.Repositories
{
	public class GenericRepository<T> : IGenericRepository<T> where T : class
	{
		protected readonly VotingDbContext _context;
		private readonly DbSet<T> _dbSet;

		public GenericRepository(VotingDbContext context)
		{
			_context = context;
			_dbSet = _context.Set<T>();
		}

		public async Task<T> GetByIdAsync(int id)
		{
			return await _dbSet.FindAsync(id);
		}

		public async Task<IEnumerable<T>> GetAllAsync()
		{
			return await _dbSet.ToListAsync();
		}

		public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
		{
			return await _dbSet.Where(expression).ToListAsync();
		}

		public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> expression)
		{
			return await _dbSet.SingleOrDefaultAsync(expression);
		}

		public async Task AddAsync(T entity)
		{
			await _dbSet.AddAsync(entity);
		}

		public async Task AddRangeAsync(IEnumerable<T> entities)
		{
			await _dbSet.AddRangeAsync(entities);
		}

		public void Update(T entity)
		{
			_dbSet.Update(entity);
		}

		public void Remove(T entity)
		{
			_dbSet.Remove(entity);
		}

		public void RemoveRange(IEnumerable<T> entities)
		{
			_dbSet.RemoveRange(entities);
		}

		public IQueryable<T> Query()
		{
			return _dbSet.AsQueryable();
		}
	}
}