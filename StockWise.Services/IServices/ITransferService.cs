using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    internal interface ITransferService
    {
        Task<IEnumerable<Transfer>> GetAllTransfersAsync();
        Task<Transfer> GetTransferByIdAsync(int id);
        Task CreateTransferAsync(Transfer transfer);
        Task UpdateTransferAsync(Transfer transfer);
        Task DeleteTransferAsync(int id);
    }
}
