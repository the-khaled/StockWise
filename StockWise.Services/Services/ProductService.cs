using AutoMapper;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.ProductDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProductResponseDto> CreateProductAsync(StandaloneProductCreateDto productDto)
        {
            
            if (productDto == null)
                throw new ArgumentNullException(nameof(productDto));

            if (string.IsNullOrWhiteSpace(productDto.Name))
                throw new BusinessException("Product name is required.");

            if (productDto.Price.Amount < 0)
                throw new BusinessException("Price cannot be negative.");

            if (productDto.InitialQuantity < 0)
                throw new BusinessException("Initial quantity cannot be negative.");

            if (productDto.ExpiryDate <= productDto.ProductionDate)
                throw new BusinessException("Expiry date must be after production date.");

            var existingProduct = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Name == productDto.Name);
            if (existingProduct != null)
                throw new BusinessException($"Product with name {productDto.Name} already exists.");

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(productDto.WarehouseId);
            if (warehouse == null)
                throw new BusinessException($"Warehouse with ID {productDto.WarehouseId} not found.");

            // Map DTO to Entity
            var product = _mapper.Map<Product>(productDto);
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var stock = new Stock
            {
                ProductId = product.Id,
                WarehouseId = productDto.WarehouseId,
                Quantity = productDto.InitialQuantity ?? 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Stocks.AddAsync(stock);
            await _unitOfWork.SaveChangesAsync();

            var createdProduct = await _unitOfWork.Products.GetByIdAsync(product.Id);
            return _mapper.Map<ProductResponseDto>(createdProduct);
        }

        public async Task<ProductResponseDto> UpdateProductAsync(int id, StandaloneProductCreateDto productDto)
        {
            if (productDto == null)
                throw new ArgumentNullException(nameof(productDto));

            if (string.IsNullOrWhiteSpace(productDto.Name))
                throw new BusinessException("Product name is required.");

            if (productDto.Price.Amount < 0)
                throw new BusinessException("Price cannot be negative.");

            if (productDto.InitialQuantity < 0)
                throw new BusinessException("Initial quantity cannot be negative.");

            if (productDto.ExpiryDate <= productDto.ProductionDate)
                throw new BusinessException("Expiry date must be after production date.");

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {id} not found.");

            var existingProduct = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Name == productDto.Name && p.Id != id);
            if (existingProduct != null)
                throw new BusinessException($"Product with name {productDto.Name} already exists.");

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(productDto.WarehouseId);
            if (warehouse == null)
                throw new BusinessException($"Warehouse with ID {productDto.WarehouseId} not found.");

            // Map DTO to existing entity
            _mapper.Map(productDto, product);
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Products.UpdateAsync(product);

            var stock = await _unitOfWork.Stocks.FirstOrDefaultAsync(s => s.ProductId == id && s.WarehouseId == productDto.WarehouseId);
            if (stock != null)
            {
                stock.Quantity = productDto.InitialQuantity ?? 0;
                stock.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Stocks.UpdateAsync(stock); 
            }
            else
            {
                var newStock = new Stock
                {
                    ProductId = product.Id,
                    WarehouseId = productDto.WarehouseId,
                    Quantity = productDto.InitialQuantity ?? 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Stocks.AddAsync(newStock);
            }

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {id} not found.");

            if (product.stocks.Any() || product.invoiceItems.Any() || product.returns.Any() || product.transfers.Any())
                throw new BusinessException("Cannot delete product because it is referenced in stocks, invoices, returns, or transfers.");

            var stocks = await _unitOfWork.Stocks.GetAllAsync(s => s.ProductId == id);
            foreach (var stock in stocks)
            {
                _unitOfWork.Stocks.Remove(stock);
            }

            await _unitOfWork.Products.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
        }

        public async Task<ProductResponseDto> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetExpiringProductsAsync(int daysBeforeExpiry)
        {
            if (daysBeforeExpiry < 0)
                throw new BusinessException("Days before expiry cannot be negative.");

            var products = await _unitOfWork.Products.GetExpiringProductsAsync(daysBeforeExpiry);
            return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetProductsByWarehouseAsync(int warehouseId)
        {
            if (warehouseId <= 0)
                throw new BusinessException("Warehouse ID must be greater than zero.");

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);
            if (warehouse == null)
                throw new BusinessException($"Warehouse with ID {warehouseId} not found.");

            var products = await _unitOfWork.Products.GetByWarehouseAsync(warehouseId);
            return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetProductsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessException("Name cannot be empty.");

            var products = await _unitOfWork.Products.GetByNameAsync(name);
            return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
        }
    }
}