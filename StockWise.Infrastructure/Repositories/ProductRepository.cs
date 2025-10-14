using Microsoft.EntityFrameworkCore;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(StockWiseDbContext context) : base(context) { }
       
        
        public override async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.stocks)
                    .ThenInclude(s => s.Warehouse)
                .Include(p => p.invoiceItems) 
                .Include(p => p.returns)      
                .Include(p => p.transfers)  
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<IEnumerable<Product>> GetExpiringProductsAsync(int daysBeforeExpiry)
        {
            var expireDate = DateTime.UtcNow.AddDays(daysBeforeExpiry);
            return await _context.Products
                .Where(p => p.ExpiryDate.HasValue && p.ExpiryDate <= expireDate && p.Condition == ProductCondition.Good)
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
    }
}
