using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.ProductDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync();
        Task <ProductResponseDto> GetProductByIdAsync(int id);
        Task<ProductResponseDto> CreateProductAsync(StandaloneProductCreateDto product);
        Task<ProductResponseDto> UpdateProductAsync(int id, StandaloneProductCreateDto product);
        Task<IEnumerable<ProductResponseDto>> GetExpiringProductsAsync(int daysBeforeExpiry);
        Task<IEnumerable<ProductResponseDto>> GetProductsByWarehouseAsync(int warehouseId);
        Task<IEnumerable<ProductResponseDto>> GetProductsByNameAsync(string name);
        Task DeleteProductAsync(int id);
    }
}
