using StockWise.Services.DTOS.InvoiceDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync();
        Task<InvoiceResponseDto> GetInvoiceByIdAsync(int id);
        Task<InvoiceResponseDto> CreateInvoiceAsync(InvoiceCreateDto invoiceDto);
        Task<InvoiceResponseDto> UpdateInvoiceAsync(int id,InvoiceCreateDto invoiceDto);
        Task DeleteInvoiceAsync(int id);
    }
}
