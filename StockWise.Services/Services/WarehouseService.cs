using AutoMapper;
using Azure;
using Microsoft.EntityFrameworkCore;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.WarehouseDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Net;
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

        public async Task<GenericResponse<IEnumerable<WarehouseResponseDto>>> GetAllWarehousesAsync()
        {
            var respons = new GenericResponse<IEnumerable<WarehouseResponseDto>>();
            var warehouses = await _unitOfWork.Warehouses.GetAllWithStocksandRepresentativesAsync();
            respons.StatusCode=(int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "The operation is successful";
            respons.Data = _mapper.Map<IEnumerable<WarehouseResponseDto>>(warehouses);
            return respons;
        }

        public async Task<GenericResponse<WarehouseResponseDto>> GetWarehouseByIdAsync(int id)
        {
            var respons = new GenericResponse<WarehouseResponseDto>();
            var warehouse = await _unitOfWork.Warehouses.GetWarehouseByIdWithStockAsync(id);
            if (warehouse == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Warehouse with ID {id} not found.";
            }
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "The operation is successful";
            respons.Data= _mapper.Map<WarehouseResponseDto>(warehouse);
            return respons;
        }

        public async Task<GenericResponse<WarehouseResponseDto>> CreateWarehouseAsync(WarehouseCreateDto warehouseDto)
        {
            var respons = new GenericResponse<WarehouseResponseDto>();
            if (warehouseDto == null)
                throw new ArgumentNullException(nameof(warehouseDto));
        
            var existingWarehouse = await _unitOfWork.Warehouses.FirstOrDefaultAsync(w => w.Name == warehouseDto.Name);
            if (existingWarehouse != null) 
            {
                respons.StatusCode= (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = $"Warehouse with name {warehouseDto.Name} already exists.";
                return respons;
            }

            var warehouse = _mapper.Map<Warehouse>(warehouseDto);
            warehouse.CreatedAt = DateTime.UtcNow;
            warehouse.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Warehouses.AddAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();

            var createdWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouse.Id);
            respons.StatusCode = (int)HttpStatusCode.Created;
            respons.Success = true;
            respons.Message = "The operation is successful";
            respons.Data=_mapper.Map<WarehouseResponseDto>(createdWarehouse);

            return respons;
        }

        public async Task<GenericResponse<WarehouseResponseDto>> UpdateWarehouseAsync(int id, WarehouseCreateDto warehouseDto)
        {
            var respons = new GenericResponse<WarehouseResponseDto>();

            if (warehouseDto == null)
                throw new ArgumentNullException(nameof(warehouseDto));
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
            if (warehouse == null) 
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Warehouse with ID {id} not found.";
                return respons;
            }

            var existingWarehouse = await _unitOfWork.Warehouses.FirstOrDefaultAsync(w => w.Name == warehouseDto.Name && w.Id != id);
            if (existingWarehouse != null) 
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = $"Warehouse with name {warehouseDto.Name} already exists.";
                return respons;
            }

            _mapper.Map(warehouseDto, warehouse);
            warehouse.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Warehouses.UpdateAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();

            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "The operation is successful";
            respons.Data = _mapper.Map<WarehouseResponseDto>(warehouse);
            return respons;
        }

        public async Task<GenericResponse<WarehouseResponseDto>> DeleteWarehouseAsync(int id)
        {
            var respons = new GenericResponse<WarehouseResponseDto>();

            //  تحميل الـ Warehouse مع العلاقات 
            var warehouse = await _unitOfWork.Warehouses
                .GetQueryable()
                .Include(w => w.Stocks)
                .Include(w => w.Representatives)
                .Include(w => w.TransfersFrom)
                .Include(w => w.TransfersTo)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warehouse == null) 
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Warehouse with ID {id} not found.";
                return respons;
            }

            if (warehouse.Stocks.Any() || warehouse.Representatives.Any() || warehouse.TransfersFrom.Any() || warehouse.TransfersTo.Any())
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Cannot delete warehouse because it is referenced in stocks, representatives, or transfers.";
                return respons;
            }

     /*       foreach (var transfer in warehouse.TransfersFrom.ToList())
                await _unitOfWork.Transfers.DeleteAsync(transfer.Id);

            foreach (var transfer in warehouse.TransfersTo.ToList())
                await _unitOfWork.Transfers.DeleteAsync(transfer.Id);*/

            await _unitOfWork.Warehouses.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "Warehouse deleted successfully.";
            respons.Data = _mapper.Map<WarehouseResponseDto>(warehouse);

            return respons;
        }

        public async Task<GenericResponse<IEnumerable<WarehouseResponseDto>>> GetWarehousesByNameAsync(string name)
        {
            var respons = new GenericResponse<IEnumerable<WarehouseResponseDto>>();

            if (string.IsNullOrWhiteSpace(name))
            {
                respons.StatusCode =(int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Name cannot be empty.";
                return respons;
            }

            var warehouses = await _unitOfWork.Warehouses.GetByNameAsync(name);
            if (warehouses == null) 
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"There is no warehouse with name {name}";
            }
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "The operation is successful";
            respons.Data = _mapper.Map<IEnumerable<WarehouseResponseDto>>(warehouses);
            return respons;
        }
    }
}