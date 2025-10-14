using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.InvoiceItemDto;
using StockWise.Services.DTOS.InvoiceItemDto.InvoiceItemDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IInvoiceItemService
    {
        Task<IEnumerable<InvoiceItemResponseDto>> GetAllInvoiceItemAsync();
        Task<InvoiceItemResponseDto> GetInvoiceItemByIdAsync(int id);
        Task<InvoiceItemResponseDto> CreateInvoiceItemAsync(InvoiceItemCreateDto dto);
        Task<InvoiceItemResponseDto> UpdateInvoiceItemAsync(int id, InvoiceItemCreateDto updateDto);
        Task DeleteInvoiceItemAsync(int id);
    }
}
