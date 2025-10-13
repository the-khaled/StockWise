using AutoMapper;
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
        private readonly IMapper _mapper;
        public ProductService(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _UnitOfWork = unitOfWork;
            _mapper=mapper;
        }
        public async Task<ProductDto> CreateProductAsync(ProductDto productdto)
        {
            if (productdto == null)
                throw new ArgumentNullException(nameof(productdto));

            if (string.IsNullOrWhiteSpace(productdto.Name))
                throw new BusinessException("Product name is required.");

            if (productdto.Price.Amount < 0)
                throw new BusinessException("Price cannot be negative.");

            if (productdto.ExpiryDate <= productdto.ProductionDate)
                throw new BusinessException("Expiry date must be after production date.");
            //هل المنتج موجود اصلا ولا لا   
            var existingProduct = await _UnitOfWork.Products.FirstOrDefaultAsync(p => p.Name == productdto.Name);
            if (existingProduct != null)
                throw new BusinessException($"Product with name {productdto.Name} already exists.");
            // Map DTO to Entity
            var product = _mapper.Map<Product>(productdto);
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _UnitOfWork.Products.AddAsync(product);
            await _UnitOfWork.SaveChangesAsync();
            var defaultWarehouseId = productdto.WarehouseId ?? 1;
            var stock = new Stock
            {
                ProductId = product.Id,
                WarehouseId = defaultWarehouseId,
                Quantity = productdto.InitialQuantity ?? 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Validate warehouse
            var warehouse = await _UnitOfWork.Warehouses.GetByIdAsync(defaultWarehouseId);
            if (warehouse == null)
                throw new BusinessException($"Warehouse with ID {defaultWarehouseId} not found.");

            await _UnitOfWork.Stocks.AddAsync(stock);
            await _UnitOfWork.SaveChangesAsync();
            // Map back to DTO
            return _mapper.Map<ProductDto>(product);

        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _UnitOfWork.Products.GetByIdAsync(id);
            if (product == null) 
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            if (product.stocks.Any())
                throw new InvalidOperationException("Cannot delete product with existing stock.");
            await _UnitOfWork.Products.DeleteAsync(id);
            await _UnitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var product= await _UnitOfWork.Products.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(product);
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var Product =await _UnitOfWork.Products.GetByIdAsync(id);
            if (Product == null) throw new KeyNotFoundException($"Product with ID {id} not found.");
            return _mapper.Map<ProductDto>(Product);
        }

        public async Task UpdateProductAsync(ProductDto productdto)
        {
            if (productdto == null)
                throw new ArgumentNullException(nameof(productdto));

            var productExist = await _UnitOfWork.Products.GetByIdAsync(productdto.Id);
            if (productExist == null)
                throw new KeyNotFoundException($"Product with ID {productdto.Id} not found.");

            _mapper.Map(productdto, productExist); // Map DTO to existing entity
            productExist.UpdatedAt = DateTime.UtcNow;

            _UnitOfWork.Products.UpdateAsync(productExist);
            await _UnitOfWork.SaveChangesAsync();
            /*  if(productdto == null) { throw new ArgumentNullException( nameof(productdto)); }

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
              await _UnitOfWork.SaveChangesAsync();*/
        }
/*        private ProductDto MapToDto(Product product)
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
        }*/

  
    }
}
