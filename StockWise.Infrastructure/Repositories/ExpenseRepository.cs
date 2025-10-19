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
    internal class ExpenseRepository:GenericRepository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(StockWiseDbContext context):base(context) { }
        public override async Task<Expense> GetByIdAsync(int id)
        {
            return await _context.expenses
                .Include(e => e.Representative)
                .FirstOrDefaultAsync(e => e.Id == id);

        }

        public async Task<IEnumerable<Expense>> GetByRepresentativeIdAsync(int representativeId)
        {
            return await _context.expenses
                .Where(e => e.RepresentativeId == representativeId)
                .ToListAsync();
        }

    }
}
