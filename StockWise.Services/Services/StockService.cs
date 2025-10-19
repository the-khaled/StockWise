using AutoMapper;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.StockDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StockService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StockResponseDto>> GetAllStocksAsync()
        {
            var stocks = await _unitOfWork.Stocks.GetAllAsync();
            return _mapper.Map<IEnumerable<StockResponseDto>>(stocks);
        }

        public async Task<StockResponseDto> GetStockByIdAsync(int id)
        {
            var stock = await _unitOfWork.Stocks.GetByIdAsync(id);
            if (stock == null)
                throw new KeyNotFoundException($"Stock with ID {id} not found.");
            return _mapper.Map<StockResponseDto>(stock);
        }

        public async Task<StockResponseDto> CreateStockAsync(StockCreateDto stockDto)
        {
            if (stockDto == null)
                throw new ArgumentNullException(nameof(stockDto));

            if (stockDto.Quantity < 0)
                throw new BusinessException("Quantity cannot be negative.");

            if (stockDto.MinQuantity < 0)
                throw new BusinessException("Minimum quantity cannot be negative.");

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(stockDto.WarehouseId);
            if (warehouse == null)
                throw new BusinessException($"Warehouse with ID {stockDto.WarehouseId} not found.");

            var product = await _unitOfWork.Products.GetByIdAsync(stockDto.ProductId);
            if (product == null)
                throw new BusinessException($"Product with ID {stockDto.ProductId} not found.");

            var existingStock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(stockDto.WarehouseId, stockDto.ProductId);
            if (existingStock != null)
                throw new BusinessException($"Stock for Warehouse ID {stockDto.WarehouseId} and Product ID {stockDto.ProductId} already exists.");


            var stock = _mapper.Map<Stock>(stockDto);
            stock.CreatedAt = DateTime.UtcNow;
            stock.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Stocks.AddAsync(stock);
            await _unitOfWork.SaveChangesAsync();

            var createdStock = await _unitOfWork.Stocks.GetByIdAsync(stock.Id);
            return _mapper.Map<StockResponseDto>(createdStock);
        }

        public async Task<StockResponseDto> UpdateStockAsync(int id, StockCreateDto stockDto)
        {
            if (stockDto == null)
                throw new ArgumentNullException(nameof(stockDto));

            if (stockDto.Quantity < 0)
                throw new BusinessException("Quantity cannot be negative.");

            if (stockDto.MinQuantity < 0)
                throw new BusinessException("Minimum quantity cannot be negative.");

            var existingStock = await _unitOfWork.Stocks.GetByIdAsync(id);
            if (existingStock == null)
                throw new KeyNotFoundException($"Stock with ID {id} not found.");

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(stockDto.WarehouseId);
            if (warehouse == null)
                throw new BusinessException($"Warehouse with ID {stockDto.WarehouseId} not found.");

            var product = await _unitOfWork.Products.GetByIdAsync(stockDto.ProductId);
            if (product == null)
                throw new BusinessException($"Product with ID {stockDto.ProductId} not found.");

            var duplicateStock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(stockDto.WarehouseId, stockDto.ProductId);
            if (duplicateStock != null && duplicateStock.Id != id)
                throw new BusinessException($"Stock for Warehouse ID {stockDto.WarehouseId} and Product ID {stockDto.ProductId} already exists.");


            _mapper.Map(stockDto, existingStock);
            existingStock.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Stocks.UpdateAsync(existingStock);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<StockResponseDto>(existingStock);
        }

        public async Task DeleteStockAsync(int id)
        {
            var stock = await _unitOfWork.Stocks.GetByIdAsync(id);
            if (stock == null)
                throw new KeyNotFoundException($"Stock with ID {id} not found.");

            await _unitOfWork.Stocks.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<StockResponseDto> GetByWarehouseAndProductAsync(int warehouseId, int productId)
        {
            var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(warehouseId, productId);
            if (stock == null)
                throw new KeyNotFoundException($"Stock for Warehouse ID {warehouseId} and Product ID {productId} not found.");
            return _mapper.Map<StockResponseDto>(stock);
        }
    }
}