using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IWarehouseService
    {
        Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync();
        Task<WarehouseDto> GetWarehouseByIdAsync(int id);
        Task CreateWarehouseAsync(WarehouseDto warehouseDto);
        Task UpdateWarehouseAsync(WarehouseDto WarehouseDto);
        Task DeleteWarehouseAsync(int id);

    }
}
