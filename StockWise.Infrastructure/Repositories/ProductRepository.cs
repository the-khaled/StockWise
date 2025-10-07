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
       /* private readonly StockWiseDbContext _context;*/
        public ProductRepository(StockWiseDbContext context):base(context) 
        {
          /*  _context = context;*/
        }
        public async Task<IEnumerable<Product>> GetExpiringProductsAsync(int daysBeforeExpiry)
        {
            var Expire= DateTime.UtcNow.AddDays(daysBeforeExpiry);
            return await _context.Products
                .Where(p => p.ExpiryDate.HasValue && p.ExpiryDate <= Expire)
                .ToListAsync();
        }
    }
}
