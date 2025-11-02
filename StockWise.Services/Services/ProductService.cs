using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Hosting;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.ProductDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Policy;
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


        public async Task<GenericResponse<ProductResponseDto>> CreateProductAsync(StandaloneProductCreateDto productDto , IWebHostEnvironment env)
        {
            var respons = new GenericResponse<ProductResponseDto>();

            if (productDto == null)
            {
                respons.StatusCode =(int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Product data required";
                return respons;
            }

            if (string.IsNullOrWhiteSpace(productDto.Name))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Product name is required.";
                return respons;
            }


            // === Image verification  ===
            if (productDto.Image == null || productDto.Image.Length == 0)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Product image is required.";
                return respons;
            }

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(productDto.Image.ContentType))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Only JPG, PNG, GIF allowed.";
                return respons;
            }

            if (productDto.Image.Length > 5 * 1024 * 1024) // 5 MB
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Image too large. Max 5MB.";
                return respons;
            }

            /////////////////////////////
            if (productDto.Price.Amount < 0)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Price cannot be negative.";
                return respons;
            }

            var allowedCurrencies = new[] { "EGP", "$", "USD", "EUR" };
            productDto.Price.Currency = productDto.Price.Currency?.Trim();

            if (string.IsNullOrWhiteSpace(productDto.Price.Currency) || !allowedCurrencies.Contains(productDto.Price.Currency))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Invalid currency , Allowed values are: EGP , $ , USD , EUR ";
                respons.Data = null;
                return respons;
            }
            if (productDto.InitialQuantity < 0)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Initial quantity cannot be negative.";
                return respons;              
            }

            if (productDto.ExpiryDate <= productDto.ProductionDate)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Expiry date must be after production date.";
                return respons;
            }
            var existingProduct = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Name == productDto.Name);
            if (existingProduct != null)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = $"Product with name {productDto.Name} already exists.";
                return respons;
            }

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(productDto.WarehouseId);
            if (warehouse == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Warehouse with ID {productDto.WarehouseId} not found.";
                return respons;
            }
            var fileName = Guid.NewGuid() + Path.GetExtension(productDto.Image.FileName);
            var filePath = Path.Combine(env.WebRootPath, "images", fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await productDto.Image.CopyToAsync(stream);

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
            var productmap = _mapper.Map<ProductResponseDto>(createdProduct);
            productmap.ImageUrl = $"/images/{fileName}";

            respons.StatusCode = (int)HttpStatusCode.Created;
            respons.Success = true;
            respons.Message = "The product has been created successfully";
            respons.Data = productmap;

            return respons;
        }

        public async Task<GenericResponse<ProductResponseDto>> UpdateProductAsync(int id, StandaloneProductCreateDto productDto, IWebHostEnvironment env)
        {
            var respons = new GenericResponse<ProductResponseDto>();

            if (productDto == null)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Product data required";
                return respons;

            }

            if (string.IsNullOrWhiteSpace(productDto.Name))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Product name is required.";
                return respons;
            }

            if (productDto.Price.Amount < 0)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Price cannot be negative.";
                return respons;
            }

            var allowedCurrencies = new[] { "EGP", "$", "USD", "EUR" };
            productDto.Price.Currency = productDto.Price.Currency?.Trim();

            if (string.IsNullOrWhiteSpace(productDto.Price.Currency) || !allowedCurrencies.Contains(productDto.Price.Currency))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Invalid currency , Allowed values are: EGP , $ , USD , EUR";
                respons.Data = null;
                return respons;
            }
            if (productDto.InitialQuantity < 0)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Initial quantity cannot be negative.";
                return respons;
            }

            if (productDto.ExpiryDate <= productDto.ProductionDate)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Expiry date must be after production date.";
                return respons;
            }

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Product with ID {id} not found.";
                return respons;
            }

            var existingProduct = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Name == productDto.Name && p.Id != id);
            if (existingProduct != null)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = $"Product with name {productDto.Name} already exists.";
                return respons;
            }

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(productDto.WarehouseId);
            if (warehouse == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Warehouse with ID {productDto.WarehouseId} not found.";
                return respons;
            }
            string? newImageUrl = product.Imag; // احتفظ بالقديمة لو مفيش جديدة

            if (productDto.Image != null)
            {
                // تحقق من الصورة
                if (productDto.Image.Length > 5 * 1024 * 1024)
                {
                    respons.StatusCode = (int)HttpStatusCode.BadRequest;
                    respons.Success = false;
                    respons.Message = "Image too large. Max 5MB.";
                    return respons;
                }

                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(productDto.Image.ContentType))
                {
                    respons.StatusCode = (int)HttpStatusCode.BadRequest;
                    respons.Success = false;
                    respons.Message = "Only JPG, PNG, GIF allowed.";
                    return respons;
                }

                // احذف الصورة القديمة
                if (!string.IsNullOrEmpty(product.Imag))
                {
                    var oldFilePath = Path.Combine(env.WebRootPath, product.Imag.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // احفظ الصورة الجديدة
                var fileName = Guid.NewGuid() + Path.GetExtension(productDto.Image.FileName);
                var filePath = Path.Combine(env.WebRootPath, "images", fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await productDto.Image.CopyToAsync(stream);

                newImageUrl = $"/images/{fileName}";
            }
            // Map DTO to existing entity
            _mapper.Map(productDto, product);
            product.UpdatedAt = DateTime.UtcNow;
            product.Imag = newImageUrl;
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
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "The product has been updated successfully";
            respons.Data = _mapper.Map<ProductResponseDto>(product);
            respons.Data.ImageUrl = respons.Data.ImageUrl; 
            return respons;
        }

        public async Task<GenericResponse<ProductResponseDto>> DeleteProductAsync(int id)
        {
            var respons =new GenericResponse<ProductResponseDto>();
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Product with ID {id} not found.";
                return respons;
            }

            if (product.stocks.Any() || product.invoiceItems.Any() || product.returns.Any() || product.transfers.Any())
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Cannot delete product because it is referenced in stocks, invoices, returns, or transfers.";
                return respons;
            }

            var stocks = await _unitOfWork.Stocks.GetAllAsync(s => s.ProductId == id);
            foreach (var stock in stocks)
            {
                _unitOfWork.Stocks.Remove(stock);
            }

            await _unitOfWork.Products.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            respons.StatusCode = (int)HttpStatusCode.NoContent;
            respons.Success = true;
            respons.Message = "The product has been successfully deleted";
            return respons;
        }

        public async Task<GenericResponse<IEnumerable<ProductResponseDto>>> GetAllProductsAsync()
        {
            var respons = new GenericResponse<IEnumerable<ProductResponseDto>>();
            var products = await _unitOfWork.Products.GetAllAsync();
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "The operation is successful";
            respons.Data = _mapper.Map<IEnumerable<ProductResponseDto>>(products);
            return respons;
        }

        public async Task<GenericResponse<ProductResponseDto>> GetProductByIdAsync(int id)
        {
            var respons = new GenericResponse<ProductResponseDto>();

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Product with ID {id} not found.";
                return respons;
            }
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "The operation is successful";
            respons.Data = _mapper.Map<ProductResponseDto>(product);
            return respons;
        }

        public async Task<GenericResponse<IEnumerable<ProductResponseDto>>> GetExpiringProductsAsync(int daysBeforeExpiry)
        {
            var respons = new GenericResponse<IEnumerable<ProductResponseDto>>();

            if (daysBeforeExpiry < 0)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = "Days before expiry cannot be negative.";
                return respons;
            }

            var products = await _unitOfWork.Products.GetExpiringProductsAsync(daysBeforeExpiry);
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "The operation is successful";
            respons.Data = _mapper.Map<IEnumerable<ProductResponseDto>>(products);
            return respons;
        }

        public async Task<GenericResponse<IEnumerable<ProductResponseDto>>> GetProductsByWarehouseAsync(int warehouseId)
        {
            var respons = new GenericResponse<IEnumerable<ProductResponseDto>>();
            if (warehouseId <= 0)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Warehouse ID must be greater than zero.";
                return respons;
            }

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);
            if (warehouse == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Warehouse with ID {warehouseId} not found.";
                return respons;
            }
            var products = await _unitOfWork.Products.GetByWarehouseAsync(warehouseId);
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "The operation is successful";
            respons.Data = _mapper.Map<IEnumerable<ProductResponseDto>>(products);
            return respons;
        }

        public async Task<GenericResponse<IEnumerable<ProductResponseDto>>> GetProductsByNameAsync(string name)
        {
            var respons = new GenericResponse<IEnumerable<ProductResponseDto>>();
            var existProduct = _unitOfWork.Products.GetByNameAsync(name);
            if (string.IsNullOrWhiteSpace(name))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Name cannot be empty.";
                return respons;
            }
            if (existProduct == null) 
            {
                respons.StatusCode = (int)HttpStatusCode.NoContent;
                respons.Success = false;
                respons.Message = $"There is no product named {name}";
                return respons;
            }
            var products = await _unitOfWork.Products.GetByNameAsync(name);
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "The operation is successful";
            respons.Data = _mapper.Map<IEnumerable<ProductResponseDto>>(products);
            return respons;

        }
    }
}