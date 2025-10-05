using Microsoft.EntityFrameworkCore;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Infrastructure.Repositories
{
    public class RepresentativeRepository : IRepresentativeRepository
    {
        private readonly StockWiseDbContext _Context;
        public RepresentativeRepository(StockWiseDbContext context)
        {
            _Context = context;
        }
        public async Task AddAsync(Representative representative)
        {
            await _Context.representatives.AddAsync(representative);
        }

        public async Task DeleteAsync(int id)
        {
            var elem= await GetByIdAsync(id);
            if (elem != null) _Context.representatives.Remove(elem);
        }

        public async Task<IEnumerable<Representative>> GetAllAsync()
        {
            return await _Context.representatives.ToListAsync();
        }

        public async Task<Representative> GetByIdAsync(int id)
        {
            return await _Context.representatives.FindAsync(id);
        }

        public async Task UpdateAsync(Representative representative)
        {
            _Context.representatives.Update(representative);
        }
    }
}
