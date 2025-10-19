using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.TransferDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface ITransferService
    {
        Task<IEnumerable<TransferResponseDto>> GetAllTransfersAsync();
        Task<TransferResponseDto> GetTransferByIdAsync(int id);
        Task<TransferResponseDto> CreateTransferAsync(TransferCreateDto transfer);
        Task<TransferResponseDto> UpdateTransferAsync(int id,TransferCreateDto transfer);
        Task<IEnumerable<TransferResponseDto>> GetByWarehouseIdAsync(int warehouseId);
        Task cancelTransferAsync(int id);
    }
}
