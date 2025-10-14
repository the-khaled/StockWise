using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.WarehouseDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WarehouseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WarehouseResponseDto>> GetAllWarehousesAsync()
        {
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
            return _mapper.Map<IEnumerable<WarehouseResponseDto>>(warehouses);
        }

        public async Task<WarehouseResponseDto> GetWarehouseByIdAsync(int id)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (warehouse == null)
                throw new KeyNotFoundException($"Warehouse with ID {id} not found.");
            return _mapper.Map<WarehouseResponseDto>(warehouse);
        }

        public async Task<WarehouseResponseDto> CreateWarehouseAsync(WarehouseCreateDto warehouseDto)
        {
            if (warehouseDto == null)
                throw new ArgumentNullException(nameof(warehouseDto));

            if (string.IsNullOrWhiteSpace(warehouseDto.Name))
                throw new BusinessException("Warehouse name is required.");

            if (string.IsNullOrWhiteSpace(warehouseDto.Address))
                throw new BusinessException("Warehouse address is required.");

            var existingWarehouse = await _unitOfWork.Warehouses.FirstOrDefaultAsync(w => w.Name == warehouseDto.Name);
            if (existingWarehouse != null)
                throw new BusinessException($"Warehouse with name {warehouseDto.Name} already exists.");

            var warehouse = _mapper.Map<Warehouse>(warehouseDto);
            warehouse.CreatedAt = DateTime.UtcNow;
            warehouse.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Warehouses.AddAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();

            var createdWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouse.Id);
            return _mapper.Map<WarehouseResponseDto>(createdWarehouse);
        }

        public async Task<WarehouseResponseDto> UpdateWarehouseAsync(int id, WarehouseCreateDto warehouseDto)
        {
            if (warehouseDto == null)
                throw new ArgumentNullException(nameof(warehouseDto));

            if (string.IsNullOrWhiteSpace(warehouseDto.Name))
                throw new BusinessException("Warehouse name is required.");

            if (string.IsNullOrWhiteSpace(warehouseDto.Address))
                throw new BusinessException("Warehouse address is required.");

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (warehouse == null)
                throw new KeyNotFoundException($"Warehouse with ID {id} not found.");

            var existingWarehouse = await _unitOfWork.Warehouses.FirstOrDefaultAsync(w => w.Name == warehouseDto.Name && w.Id != id);
            if (existingWarehouse != null)
                throw new BusinessException($"Warehouse with name {warehouseDto.Name} already exists.");

            _mapper.Map(warehouseDto, warehouse);
            warehouse.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Warehouses.UpdateAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<WarehouseResponseDto>(warehouse);
        }

        public async Task DeleteWarehouseAsync(int id)
        {
            //  تحميل الـ Warehouse مع العلاقات 
            var warehouse = await _unitOfWork.Warehouses
                .GetQueryable()
                .Include(w => w.Stocks)
                .Include(w => w.Representatives)
                .Include(w => w.TransfersFrom)
                .Include(w => w.TransfersTo)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warehouse == null)
                throw new KeyNotFoundException($"Warehouse with ID {id} not found.");

            if (warehouse.Stocks.Any() || warehouse.Representatives.Any() || warehouse.TransfersFrom.Any() || warehouse.TransfersTo.Any())
                throw new BusinessException("Cannot delete warehouse because it is referenced in stocks, representatives, or transfers.");

            foreach (var transfer in warehouse.TransfersFrom.ToList())
                await _unitOfWork.Transfers.DeleteAsync(transfer.Id);

            foreach (var transfer in warehouse.TransfersTo.ToList())
                await _unitOfWork.Transfers.DeleteAsync(transfer.Id);

            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.Warehouses.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<WarehouseResponseDto>> GetWarehousesByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessException("Name cannot be empty.");

            var warehouses = await _unitOfWork.Warehouses.GetByNameAsync(name);
            return _mapper.Map<IEnumerable<WarehouseResponseDto>>(warehouses);
        }
    }
}