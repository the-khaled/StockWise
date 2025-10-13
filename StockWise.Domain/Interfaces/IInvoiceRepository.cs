using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Domain.Interfaces
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status);
        Task<IEnumerable<Invoice>> GetAllWithItemsAsync();
        Task<Invoice> GetByIdWithItemsAsync(int id);
    }
}
