using StockWise.Services.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync();
        Task<InvoiceDto> GetInvoiceByIdAsync(int id);
        Task<InvoiceDto> CreateInvoiceAsync(InvoiceDto invoiceDto);
        Task<InvoiceDto> UpdateInvoiceAsync(InvoiceDto invoiceDto);
        Task DeleteInvoiceAsync(int id);
    }
}
