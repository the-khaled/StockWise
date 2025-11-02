using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StockWise.Domain.Interfaces
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<IEnumerable<Invoice>> GetByStatusAsync(Enums.InvoiceStatus status);
        Task<IEnumerable<Invoice>> GetAllWithItemsAsync();
        Task<Invoice> GetByIdWithItemsAsync(int id);
    }
}
