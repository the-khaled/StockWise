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
    public class StockRepository : GenericRepository<Stock>, IStockRepository
    {
        public StockRepository(StockWiseDbContext context):base(context) { }
        public override async Task<Stock> GetByIdAsync(int id)
        {
            return await _context.stocks
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<Stock> GetByWarehouseAndProductAsync(int warehouseId, int productId)
        {
            return await _context.stocks
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId);
        }
    }
}
