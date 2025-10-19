using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Interfaces
{
    public interface ITransferRepository :IRepository<Transfer>
    {
        Task<Transfer> GetByIdAsync(int id);
        Task<IEnumerable<Transfer>> GetAllAsync();
        Task<IEnumerable<Transfer>> GetByFromWarehouseIdAsync(int warehouseId);
        Task<IEnumerable<Transfer>> GetByToWarehouseIdAsync(int warehouseId);
        Task<IEnumerable<Transfer>> GetByProductIdAsync(int productId);
    }
}
