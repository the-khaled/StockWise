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
    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;
        public StockService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreateStockAsync(StockDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Quantity < 0)
                throw new BusinessException("Quantity cannot be negative.");

            if (dto.MinQuantity < 0)
                throw new BusinessException("Minimum quantity cannot be negative.");

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null)
                throw new BusinessException("Warehouse not found.");

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new BusinessException("Product not found.");

            await _unitOfWork.Stocks.AddAsync(MapToEntity(dto));
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteStockAsync(int id)
        {
            var stock = await _unitOfWork.Stocks.GetByIdAsync(id);
            if (stock == null)
                throw new KeyNotFoundException($"Stock with ID {id} not found.");

            await _unitOfWork.Stocks.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<StockDto>> GetAllStocksAsync()
        {
            var stocks = await _unitOfWork.Stocks.GetAllAsync();
            return stocks.Select(s => MapToDto(s)).ToList();
        }

        public async Task<StockDto> GetStockByIdAsync(int id)
        {
            var stock = await _unitOfWork.Stocks.GetByIdAsync(id);
            if (stock == null)
                throw new KeyNotFoundException($"Stock with ID {id} not found.");
            return MapToDto(stock);
        }

        public async Task UpdateStockAsync(StockDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var existingStock = await _unitOfWork.Stocks.GetByIdAsync(dto.Id);
            if (existingStock == null)
                throw new KeyNotFoundException($"Stock with ID {dto.Id} not found.");

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null)
                throw new BusinessException("Warehouse not found.");

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new BusinessException("Product not found.");

            existingStock.WarehouseId = dto.WarehouseId;
            existingStock.ProductId = dto.ProductId;
            existingStock.Quantity = dto.Quantity;
            existingStock.MinQuantity = dto.MinQuantity;
            existingStock.UpdatedAt = DateTime.Now;

            await _unitOfWork.Stocks.UpdateAsync(existingStock);
            await _unitOfWork.SaveChangesAsync();
        }
        private StockDto MapToDto(Stock stock)
        {
            return new StockDto
            {
                Id = stock.Id,
                WarehouseId = stock.WarehouseId,
                ProductId = stock.ProductId,
                Quantity = stock.Quantity,
                MinQuantity = stock.MinQuantity,
                CreatedAt = stock.CreatedAt,
                UpdatedAt = stock.UpdatedAt
            };
        }

        private Stock MapToEntity(StockDto dto)
        {
            return new Stock
            {
                Id = dto.Id,
                WarehouseId = dto.WarehouseId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                MinQuantity = dto.MinQuantity,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }
}

