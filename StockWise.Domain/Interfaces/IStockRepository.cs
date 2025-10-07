using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Interfaces
{
    public interface IStockRepository : IRepository<Stock>
    {
        Task<Stock> GetByWarehouseAndProductAsync(int warehouseId, int productId);
    
    }
}

