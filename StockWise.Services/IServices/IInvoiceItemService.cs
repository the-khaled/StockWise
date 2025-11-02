using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.InvoiceItemDto;
using StockWise.Services.DTOS.InvoiceItemDto.InvoiceItemDto;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IInvoiceItemService
    {
        Task<GenericResponse<IEnumerable<InvoiceItemResponseDto>>> GetAllInvoiceItemAsync();
        Task<GenericResponse<InvoiceItemResponseDto>> GetInvoiceItemByIdAsync(int id);
        Task<GenericResponse<InvoiceItemResponseDto>> CreateInvoiceItemAsync(InvoiceItemCreateDto dto);
        Task<GenericResponse<InvoiceItemResponseDto>> UpdateInvoiceItemAsync(int id, InvoiceItemCreateDto updateDto);
        Task<GenericResponse<InvoiceItemResponseDto>> DeleteInvoiceItemAsync(int id);
    }
}
