using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface ITransferService
    {
        Task<IEnumerable<TransferDto>> GetAllTransfersAsync();
        Task<TransferDto> GetTransferByIdAsync(int id);
        Task CreateTransferAsync(TransferDto transfer);
        Task UpdateTransferAsync(TransferDto transfer);
        Task DeleteTransferAsync(int id);
    }
}
