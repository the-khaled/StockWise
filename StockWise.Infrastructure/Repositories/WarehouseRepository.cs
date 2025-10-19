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
    public class WarehouseRepository:GenericRepository<Warehouse>,IWarehouseRepository
    {
        public WarehouseRepository(StockWiseDbContext context) : base(context) { }

        public async Task<IEnumerable<Warehouse>> GetAllWithStocksandRepresentativesAsync()
        {
            return await _context.Set<Warehouse>()
                .Include(s=>s.Stocks)
                .Include(r=>r.Representatives)
                .ToListAsync();
        }
        public async Task<Warehouse> GetWarehouseByIdWithStockAsync(int id)
        {
            return await _context.Set<Warehouse>().Where(w=>w.Id==id).Include(s => s.Stocks)
                .Include(r => r.Representatives).FirstOrDefaultAsync(w => w.Id == id); 
        }
        public async Task<IEnumerable<Warehouse>> GetByNameAsync(string name)
        {
            return await _context.warehouses
                .Include(s => s.Stocks)
                .Include(r => r.Representatives)
                .Where(w => w.Name.Contains(name))
                .ToListAsync();
        }

    }
}
