using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _UnitOfWork;
        public ProductService(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;   
        }
        public async Task<ProductDto> CreateProductAsync(ProductDto productdto)
        {
            if (productdto == null)
                throw new ArgumentNullException(nameof(productdto));

            if (productdto.Price.Amount < 0)
                throw new BusinessException("Price cannot be negative.");

            if (productdto.ExpiryDate <= productdto.ProductionDate)
                throw new BusinessException("Expiry date must be after production date.");
            var productm = MapToEntity(productdto);
            await _UnitOfWork.Products.AddAsync(productm);
            await _UnitOfWork.SaveChangesAsync();
            return MapToDto(productm);
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _UnitOfWork.Products.GetByIdAsync(id);
            if (product == null) 
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            await _UnitOfWork.Products.DeleteAsync(id);
            await _UnitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var product= await _UnitOfWork.Products.GetAllAsync();
            return product.Select(p=> MapToDto(p)).ToList();
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var Product =await _UnitOfWork.Products.GetByIdAsync(id);
            if (Product == null) throw new KeyNotFoundException($"Product with ID {id} not found.");
            return MapToDto(Product);
        }

        public async Task UpdateProductAsync(ProductDto productdto)
        {   
            if(productdto == null) { throw new ArgumentNullException( nameof(productdto)); }
 
            var productexist = await _UnitOfWork.Products.GetByIdAsync(productdto.Id);
            if (productexist == null)
                throw new KeyNotFoundException($"Product with ID {productdto.Id} not found.");
            productexist.Name = productdto.Name;
            productexist.Price = productdto.Price;
            productexist.Condition = productdto.Condition;
            productexist.ProductionDate = productdto.ProductionDate;
            productexist.ExpiryDate = productdto.ExpiryDate;
            productexist.UpdatedAt = DateTime.Now;
            await _UnitOfWork.Products.UpdateAsync(productexist);
            await _UnitOfWork.SaveChangesAsync();
        }
        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                ProductionDate = (DateTime)product.ProductionDate,
                ExpiryDate = (DateTime)product.ExpiryDate,
                Price = product.Price,
                Condition = product.Condition,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        private Product MapToEntity(ProductDto dto)
        {
            return new Product
            {
                Id = dto.Id,
                Name = dto.Name,
                ProductionDate = dto.ProductionDate,
                ExpiryDate = dto.ExpiryDate,
                Price = dto.Price,
                Condition = dto.Condition,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }
}
