using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IInvoiceItemService
    {
        Task<IEnumerable<InvoiceItemDto>> GetAllInvoiceItemAsync();
        Task<InvoiceItemDto> GetInvoiceItemByIdAsync(int id);
        Task CreateInvoiceItemAsync(InvoiceItemDto dto);
        Task UpdateInvoiceItemAsync(InvoiceItemDto dto);
        Task DeleteInvoiceItemAsync(int id);
    }
}
