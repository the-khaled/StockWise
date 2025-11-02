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
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(StockWiseDbContext context) : base(context) { }
       
        
        public override async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
             .Include(p => p.stocks) 
             .AsNoTracking()
             .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<IEnumerable<Product>> GetExpiringProductsAsync(int daysBeforeExpiry)
        {
            var expireDate = DateTime.UtcNow.AddDays(daysBeforeExpiry);
            return await _context.Products
                .Where(p => p.ExpiryDate.HasValue && p.ExpiryDate <= expireDate && p.Condition == Domain.Enums.ProductCondition.Good)
                .Include(p => p.stocks)
                    .ThenInclude(s => s.Warehouse)
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetByWarehouseAsync(int warehouseId)
        {
            return await _context.Products
                .Include(p => p.stocks)
                .Where(p => p.stocks.Any(s => s.WarehouseId == warehouseId))
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetByNameAsync(string name)
        {
            return await _context.Products
                .Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .Include(p => p.stocks)
                    .ThenInclude(s => s.Warehouse)
                .ToListAsync();
        }
        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
             .Include(p => p.stocks)
             .AsNoTracking()
             .ToListAsync();
        }
    }
}
