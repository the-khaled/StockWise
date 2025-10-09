using Microsoft.EntityFrameworkCore;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        public WarehouseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreateWarehouseAsync(WarehouseDto warehousedto)
        {
            
            if (warehousedto == null) throw new ArgumentNullException(nameof(warehousedto));
            await _unitOfWork.Warehouses.AddAsync(MapToEntity(warehousedto));   
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteWarehouseAsync(int id)
        {

            // تحميل الـ Warehouse بكل العلاقات المرتبطة
            var warehouse = await _unitOfWork.Warehouses
                .GetQueryable()
                .Include(w => w.TransfersFrom)
                .Include(w => w.TransfersTo)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warehouse == null)
                throw new KeyNotFoundException($"Warehouse with ID {id} not found");

            // حذف التحويلات المرتبطة قبل حذف المستودع
            foreach (var transfer in warehouse.TransfersFrom.ToList())
                await _unitOfWork.Transfers.DeleteAsync(transfer.Id);

            foreach (var transfer in warehouse.TransfersTo.ToList())
                await _unitOfWork.Transfers.DeleteAsync(transfer.Id);

            await _unitOfWork.SaveChangesAsync();

            // حذف المستودع نفسه
            await _unitOfWork.Warehouses.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            /*  var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
              if (warehouse == null)
                  throw new KeyNotFoundException($"warehouse with ID {id} Not found");
          // حذف التحويلات المرتبطة قبل حذف المستودع
          var transfersFrom = warehouse.TransfersFrom.ToList();
          var transfersTo = warehouse.TransfersTo.ToList();

          foreach (var transfer in transfersFrom.Concat(transfersTo))
              await _unitOfWork.Transfers.DeleteAsync(transfer.Id);

          await _unitOfWork.SaveChangesAsync();
          await _unitOfWork.Warehouses.DeleteAsync(id);
              await _unitOfWork.SaveChangesAsync();*/
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync()
        {
            
                var warehouse = await _unitOfWork.Warehouses.GetAllAsync();
                return warehouse.Select(p => MapToDto(p)).ToList();
            
        }

        public async Task<WarehouseDto> GetWarehouseByIdAsync(int id)
        {
           
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id);
                if (warehouse == null)
                    throw new KeyNotFoundException($"warehouse with ID {id} Not found");
                return MapToDto(warehouse);
           
        }

        public async Task UpdateWarehouseAsync(WarehouseDto WarehouseDto)
        {
            if (WarehouseDto == null) throw new ArgumentNullException(nameof(WarehouseDto));
            var warehouseexist = await _unitOfWork.Warehouses.GetByIdAsync(WarehouseDto.Id);
            if (warehouseexist == null) 
                throw new KeyNotFoundException($"Warehouse With ID {WarehouseDto.Id} not found");

            warehouseexist.Name = WarehouseDto.Name;
            warehouseexist.WarehouseType = WarehouseDto.WarehouseType;
            warehouseexist.Address = WarehouseDto.Address;
            warehouseexist.UpdatedAt = DateTime.Now;
            await _unitOfWork.Warehouses.UpdateAsync(warehouseexist);
            await _unitOfWork.SaveChangesAsync();

        }

        public WarehouseDto MapToDto(Warehouse warehouse)
        {
            return new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                WarehouseType = warehouse.WarehouseType,
                Address = warehouse.Address,
                CreatedAt = warehouse.CreatedAt,
                UpdatedAt = warehouse.UpdatedAt
            };
        }
        public Warehouse MapToEntity(WarehouseDto warehouseDto)
        {
            return new Warehouse
            {
                Id= warehouseDto.Id,
                Name = warehouseDto.Name,
                WarehouseType = warehouseDto.WarehouseType,
                Address = warehouseDto.Address,
                UpdatedAt= warehouseDto.UpdatedAt,
                CreatedAt= warehouseDto.CreatedAt,
            };
        }
    }
}
