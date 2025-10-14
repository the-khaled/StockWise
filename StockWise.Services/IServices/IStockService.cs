using StockWise.Services.DTOS;
using StockWise.Services.DTOS.StockDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IStockService
    {
        Task<IEnumerable<StockResponseDto>> GetAllStocksAsync();
        Task<StockResponseDto> GetStockByIdAsync(int id);
        Task<StockResponseDto> CreateStockAsync(StockCreateDto dto);
        Task<StockResponseDto> UpdateStockAsync(int id, StockCreateDto dto);
        Task<StockResponseDto> GetByWarehouseAndProductAsync(int warehouseId, int productId);
        Task DeleteStockAsync(int id);
    }
}
