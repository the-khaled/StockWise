using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Interfaces
{
    public interface IProductRepository:IRepository<Product>
    {
        Task<IEnumerable<Product>> GetExpiringProductsAsync(int daysBeforeExpiry);
        Task<IEnumerable<Product>> GetByWarehouseAsync(int warehouseId);
        Task<IEnumerable<Product>> GetByNameAsync(string name);
    }
}
