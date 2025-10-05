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
    public class StockRepository : IStockRepository
    {
        private readonly StockWiseDbContext _context;
        public StockRepository(StockWiseDbContext context)
        {
                _context = context;
        }
        public async Task AddAsync(Stock stock)
        {

            await _context.stocks.AddAsync(stock);
        }

        public async Task DeleteAsync(int warehouseId, int productId)
        {
            var stock = await GetByIdAsync(warehouseId, productId);
            if (stock != null) _context.stocks.Remove(stock);
        }

        public async Task<IEnumerable<Stock>> GetAllAsync()
        {
            return await _context.Set<Stock>().ToListAsync();
        }

        public async Task<Stock> GetByIdAsync(int warehouseId, int productId)
        {
            return await _context.stocks
                 .FirstOrDefaultAsync
                 (s => s.WarehouseId == warehouseId && s.ProductId == productId);
        }

        public async Task UpdateAsync(Stock stock)
        {
           _context.stocks.Update(stock);
        }
    }
}
