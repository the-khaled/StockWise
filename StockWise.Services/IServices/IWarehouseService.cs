using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.WarehouseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IWarehouseService
    {
        Task<IEnumerable<WarehouseResponseDto>> GetAllWarehousesAsync();
        Task<WarehouseResponseDto> GetWarehouseByIdAsync(int id);
        Task<WarehouseResponseDto> CreateWarehouseAsync(WarehouseCreateDto warehouseDto);
        Task<WarehouseResponseDto> UpdateWarehouseAsync(int id, WarehouseCreateDto warehouseDto);
        Task DeleteWarehouseAsync(int id);
        
        Task<IEnumerable<WarehouseResponseDto>> GetWarehousesByNameAsync(string name);

    }
}
