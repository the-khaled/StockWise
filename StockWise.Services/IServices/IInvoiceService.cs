using StockWise.Services.DTOS.InvoiceDto;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IInvoiceService
    {
        Task<GenericResponse<IEnumerable<InvoiceResponseDto>>> GetAllInvoicesAsync();
        Task<GenericResponse<InvoiceResponseDto>> GetInvoiceByIdAsync(int id);
        Task<GenericResponse<InvoiceResponseDto>> CreateInvoiceAsync(InvoiceCreateDto invoiceDto);
        Task<GenericResponse<InvoiceResponseDto>> UpdateInvoiceAsync(int id,InvoiceCreateDto invoiceDto);

        Task<GenericResponse<InvoiceResponseDto>> DeleteInvoiceAsync(int id);
    }
}
