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
    public class InvoiceRepository:GenericRepository<Invoice>,IInvoiceRepository
    {
        public InvoiceRepository(StockWiseDbContext context):base(context) { }
        public async Task<IEnumerable<Invoice>> GetAllWithItemsAsync()
        {
            return await _context.invoices
                .Include(i => i.Items) // دي اللي بتخلي EF يجيب الـ Items مع الفاتورة
                .ToListAsync();
        }
        public async Task<Invoice> GetByIdWithItemsAsync(int id)
        {
            return await _context.invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

    }
}
