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
    public class ReturnRepository:GenericRepository<Return>,IReturnRepository
    {    
        public ReturnRepository(StockWiseDbContext context):base(context) { }
        public override async Task<Return> GetByIdAsync(int id)
        {
            return await _context.returns
                .Include(x => x.Product)
                .ThenInclude(x=>x.stocks)
                .Include(x => x.Representative)
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
            public async Task<IEnumerable<Return>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.returns.Where(x => x.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Return>> GetByProductIdAsync(int productId)
        {
            return await _context.returns.Where(x=>x.ProductId==productId).ToListAsync();
        }

        public async Task<IEnumerable<Return>> GetByRepresentativeIdAsync(int representativeId)
        {
            return await _context.returns.Where(x => x.RepresentativeId == representativeId).ToListAsync();
        }
    }
}
