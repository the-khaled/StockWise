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
    public class TransferRepository : GenericRepository<Transfer>,ITransferRepository
    {
        public TransferRepository(StockWiseDbContext context) : base(context) { }
        public async Task<Transfer> GetByIdAsync(int id)
        {
            return await _context.transfers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transfer>> GetAllAsync()
        {
            return await _context.transfers
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Transfer>> GetByFromWarehouseIdAsync(int warehouseId)
        {
            return await _context.transfers
                .AsNoTracking()
                .Where(t => t.FromWarehouseId == warehouseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transfer>> GetByToWarehouseIdAsync(int warehouseId)
        {
            return await _context.transfers
                .AsNoTracking()
                .Where(t => t.ToWarehouseId == warehouseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transfer>> GetByProductIdAsync(int productId)
        {
            return await _context.transfers
                .AsNoTracking()
                .Where(t => t.ProductId == productId)
                .ToListAsync();
        }
    }
}
