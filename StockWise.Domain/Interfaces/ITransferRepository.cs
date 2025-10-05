using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Interfaces
{
    public interface ITransferRepository
    {
        Task<Transfer> GetByIdAsync(int id);
        Task<IEnumerable<Transfer>> GetAllAsync();
        Task AddAsync(Transfer transfer);
        Task UpdateAsync(Transfer transfer);
        Task DeleteAsync(int id);
    }
}
