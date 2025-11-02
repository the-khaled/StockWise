using Microsoft.EntityFrameworkCore;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Infrastructure.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly StockWiseDbContext _context;
        public GenericRepository(StockWiseDbContext context)
        {
            _context=context;
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }
        public virtual async Task<T> GetByIdAsync(int id)
        {
            return  await _context.Set<T>().FindAsync(id);
        }
        public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var en = await GetByIdAsync(id);
            if (en != null) 
                _context.Set<T>().Remove(en);
            
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
        }
        public IQueryable<T> GetQueryable()
        {
            return _context.Set<T>().AsQueryable();
        }
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)// to search with name 
        {
            return await _context.Set<T>().AnyAsync(predicate);
        }
        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }
        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
    }
}
