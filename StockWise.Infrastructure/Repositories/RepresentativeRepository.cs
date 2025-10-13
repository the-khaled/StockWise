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
    public class RepresentativeRepository : GenericRepository<Representative>,IRepresentativeRepository
    {
        public RepresentativeRepository(StockWiseDbContext context):base(context) { }
        public override async Task<Representative> GetByIdAsync(int id)
        {
            return await _context.representatives
                .Include(r => r.Warehouse)
                .Include(r => r.Invoices)
                .Include(r => r.Returns)
                .Include(r => r.Expenses)
                .Include(r => r.Locations)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
