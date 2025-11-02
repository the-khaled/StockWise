using Microsoft.AspNetCore.Hosting;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.ProductDto;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IProductService
    {
        Task<GenericResponse<IEnumerable<ProductResponseDto>>> GetAllProductsAsync();
        Task <GenericResponse<ProductResponseDto>> GetProductByIdAsync(int id);
        Task<GenericResponse<ProductResponseDto>> CreateProductAsync(StandaloneProductCreateDto product, IWebHostEnvironment env);
        Task<GenericResponse<ProductResponseDto>> UpdateProductAsync(int id, StandaloneProductCreateDto product, IWebHostEnvironment env);
        Task<GenericResponse<IEnumerable<ProductResponseDto>>> GetExpiringProductsAsync(int daysBeforeExpiry);
        Task<GenericResponse<IEnumerable<ProductResponseDto>>> GetProductsByWarehouseAsync(int warehouseId);
        Task<GenericResponse<IEnumerable<ProductResponseDto>>> GetProductsByNameAsync(string name);
        Task<GenericResponse<ProductResponseDto>> DeleteProductAsync(int id);
    }
}
