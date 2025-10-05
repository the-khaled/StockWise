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
    public class TransferRepository : ITransferRepository
    {
        private readonly StockWiseDbContext _context;

        public TransferRepository(StockWiseDbContext context)
        {
            _context = context;
        }

        public async Task<Transfer> GetByIdAsync(int id)
        {
            return await _context.transfers.FindAsync(id);
        }

        public async Task<IEnumerable<Transfer>> GetAllAsync()
        {
            return await _context.transfers.ToListAsync();
        }

        public async Task AddAsync(Transfer transfer)
        {
            await _context.transfers.AddAsync(transfer);
        }

        public async Task UpdateAsync(Transfer transfer)
        {
            _context.transfers.Update(transfer);
        }

        public async Task DeleteAsync(int id)
        {
            var transfer = await GetByIdAsync(id);
            if (transfer != null)
                _context.transfers.Remove(transfer);
        }
    }
}
