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
    public class InvoiceRepository:GenericRepository<Invoice>,IInvoiceRepository
    {
        public InvoiceRepository(StockWiseDbContext context):base(context) { }
        public override async Task<Invoice> GetByIdAsync(int id)
        {
            return await _context.invoices
                .Include(i => i.Items )
                    .ThenInclude(ii => ii.Product)
                .Include(i => i.Customer)
                .Include(i => i.Representative)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
        public async Task<IEnumerable<Invoice>> GetAllWithItemsAsync()
        {
            return await _context.invoices
             .Include(i => i.Items)
                 .ThenInclude(ii => ii.Product)
             .Include(i => i.Customer)
             .Include(i => i.Representative)
             .ToListAsync();
        }
        public async Task<Invoice> GetByIdWithItemsAsync(int id)
        {
            return await _context.invoices
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.Product)
                .Include(i => i.Customer)
                .Include(i => i.Representative)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
        public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status)
        {
            return await _context.invoices
                .Include(i => i.Items)
                .Include(i => i.Customer)
                .Include(i => i.Representative)
                .Where(i => i.Status  == status)
                .ToListAsync();
        }

    }
}
