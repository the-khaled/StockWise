using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.WarehouseDto;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IWarehouseService
    {
        Task<GenericResponse<IEnumerable<WarehouseResponseDto>>> GetAllWarehousesAsync();
        Task<GenericResponse<WarehouseResponseDto>> GetWarehouseByIdAsync(int id);
        Task<GenericResponse<WarehouseResponseDto>> CreateWarehouseAsync(WarehouseCreateDto warehouseDto);
        Task<GenericResponse<WarehouseResponseDto>> UpdateWarehouseAsync(int id, WarehouseCreateDto warehouseDto);
        Task <GenericResponse<WarehouseResponseDto>> DeleteWarehouseAsync(int id);
        
        Task<GenericResponse<IEnumerable<WarehouseResponseDto>>> GetWarehousesByNameAsync(string name);

    }
}
