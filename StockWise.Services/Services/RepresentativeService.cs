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
    public class RepresentativeService : IRepresentativeService
    {
        private readonly IUnitOfWork _unitOfWork;
        public RepresentativeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreateRepresentativeAsync(RepresentativeDto Representativedto)
        {
           if (Representativedto == null) throw new ArgumentNullException(nameof(Representativedto));
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(Representativedto.WarehouseId);
            if (warehouse == null) throw new BusinessException("Warehouse not found.");
            if(!string.IsNullOrEmpty(Representativedto.NationalId) && Representativedto.NationalId.Length != 14)
                throw new BusinessException("National ID must be 14 characters.");
            await _unitOfWork.Representatives.AddAsync(MapToEntity(Representativedto));
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteRepresentativeAsync(int id)
        {
            var existRepresentativedto = await _unitOfWork.Representatives.GetByIdAsync(id);
            if (existRepresentativedto == null)
                throw new KeyNotFoundException($"Representative with ID {id} not found.");
            await _unitOfWork.Representatives.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<RepresentativeDto>> GetAllRepresentativeAsync()
        {
            var Representative = await _unitOfWork.Representatives.GetAllAsync();
            return Representative.Select(p=>MapToDto(p)).ToList();
        }

        public async Task<RepresentativeDto> GetRepresentativeByIdAsync(int id)
        {
            var Representative = await _unitOfWork.Representatives.GetByIdAsync(id);
            if(Representative ==null) throw new KeyNotFoundException($"Representative with ID {id} not found.");
            return MapToDto(Representative);
        }

        public async Task UpdateRepresentativeAsync(RepresentativeDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var existingRepresentative = await _unitOfWork.Representatives.GetByIdAsync(dto.Id);
            if (existingRepresentative == null)
                throw new KeyNotFoundException($"Representative with ID {dto.Id} not found.");

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null)
                throw new BusinessException("Warehouse not found.");
            existingRepresentative.Name = dto.Name;
            existingRepresentative.NationalId = dto.NationalId;
            existingRepresentative.PhoneNumber = dto.PhoneNumber;
            existingRepresentative.Address = dto.Address;
            existingRepresentative.Notes = dto.Notes;
            existingRepresentative.WarehouseId = dto.WarehouseId;
            existingRepresentative.UpdatedAt = DateTime.Now;

            await _unitOfWork.Representatives.UpdateAsync(existingRepresentative);
            await _unitOfWork.SaveChangesAsync();
        }
        public RepresentativeDto MapToDto(Representative Representative) 
        {
            return new RepresentativeDto 
            {
                Id = Representative.Id,
                Name = Representative.Name,
                NationalId = Representative.NationalId,
                PhoneNumber = Representative.PhoneNumber.ToList(),
                Address = Representative.Address,
                Notes = Representative.Notes,
                WarehouseId = Representative.WarehouseId,
                CreatedAt = Representative.CreatedAt,
                UpdatedAt = Representative.UpdatedAt
            };
        }
        public Representative MapToEntity(RepresentativeDto Representativedto)
        {
            return new Representative
            {
                Id = Representativedto.Id,
                Name = Representativedto.Name,
                NationalId = Representativedto.NationalId,
                PhoneNumber = Representativedto.PhoneNumber.ToList(),
                Address = Representativedto.Address,
                Notes = Representativedto.Notes,
                WarehouseId = Representativedto.WarehouseId,
                CreatedAt = Representativedto.CreatedAt,
                UpdatedAt = Representativedto.UpdatedAt
            };
        }
    }
}