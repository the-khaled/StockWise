using AutoMapper;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.RepresentativeDto;
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
        private readonly IMapper _mapper;
        public RepresentativeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<RepresentativeResponseDto> CreateRepresentativeAsync(RepresentativeCreateDto Representativedto)
        {
            if (Representativedto == null)
                throw new ArgumentNullException(nameof(Representativedto));

            // Validate WarehouseId
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(Representativedto.WarehouseId);
            if (warehouse == null)
                throw new BusinessException($"Warehouse with ID {Representativedto.WarehouseId} not found.");

            // Validate NationalId uniqueness if provided
            if (!string.IsNullOrWhiteSpace(Representativedto.NationalId))
            {
                var existingRepresentative = await _unitOfWork.Representatives.GetByNationalIdAsync(Representativedto.NationalId);
                if (existingRepresentative != null)
                    throw new BusinessException($"Representative with National ID {Representativedto.NationalId} already exists.");
            }

            // Validate PhoneNumber
            if (!Representativedto.PhoneNumber.Any())
                throw new BusinessException("At least one phone number is required.");

            var representative = _mapper.Map<Representative>(Representativedto);
            representative.CreatedAt = DateTime.UtcNow;
            representative.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Representatives.AddAsync(representative);
            await _unitOfWork.SaveChangesAsync();

            var createdRepresentative = await _unitOfWork.Representatives.GetByIdAsync(representative.Id);
            return _mapper.Map<RepresentativeResponseDto>(createdRepresentative);
        }

        public async Task DeleteRepresentativeAsync(int id)
        {
            var representative = await _unitOfWork.Representatives.GetByIdAsync(id);
            if (representative == null)
                throw new KeyNotFoundException($"Representative with ID {id} not found.");

            // Check for related entities
            if (representative.Invoices.Any() || representative.Returns.Any() || representative.Locations.Any() || representative.Expenses.Any())
                throw new BusinessException("Cannot delete representative with associated invoices, returns, locations, or expenses.");

            await _unitOfWork.Representatives.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<RepresentativeResponseDto>> GetAllRepresentativeAsync()
        {
            var representatives = await _unitOfWork.Representatives.GetAllAsync();
            return _mapper.Map<IEnumerable<RepresentativeResponseDto>>(representatives);
        }

        public async Task<RepresentativeResponseDto> GetRepresentativeByIdAsync(int id)
        {
            var representative = await _unitOfWork.Representatives.GetByIdAsync(id);
            if (representative == null)
                throw new KeyNotFoundException($"Representative with ID {id} not found.");
            return _mapper.Map<RepresentativeResponseDto>(representative);
        }

        public async Task<RepresentativeResponseDto> UpdateRepresentativeAsync(int id, RepresentativeCreateDto representativeDto)
        {
            if (representativeDto == null)
                throw new ArgumentNullException(nameof(representativeDto));

            var existingRepresentative = await _unitOfWork.Representatives.GetByIdAsync(id);
            if (existingRepresentative == null)
                throw new KeyNotFoundException($"Representative with ID {id} not found.");

            // Validate WarehouseId
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(representativeDto.WarehouseId);
            if (warehouse == null)
                throw new BusinessException($"Warehouse with ID {representativeDto.WarehouseId} not found.");

            // Validate NationalId uniqueness if provided
            if (!string.IsNullOrWhiteSpace(representativeDto.NationalId))
            {
                var existingByNationalId = await _unitOfWork.Representatives.GetByNationalIdAsync(representativeDto.NationalId);
                if (existingByNationalId != null && existingByNationalId.Id != id)
                    throw new BusinessException($"Representative with National ID {representativeDto.NationalId} already exists.");
            }

            // Validate PhoneNumber
            if (!representativeDto.PhoneNumber.Any())
                throw new BusinessException("At least one phone number is required.");

            _mapper.Map(representativeDto, existingRepresentative);
            existingRepresentative.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Representatives.UpdateAsync(existingRepresentative);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<RepresentativeResponseDto>(existingRepresentative);
        }
        public async Task<IEnumerable<RepresentativeResponseDto>> GetRepresentativesByWarehouseIdAsync(int warehouseId)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);
            if (warehouse == null)
                throw new KeyNotFoundException($"Warehouse with ID {warehouseId} not found.");

            var representatives = await _unitOfWork.Representatives.GetByWarehouseIdAsync(warehouseId);
            return _mapper.Map<IEnumerable<RepresentativeResponseDto>>(representatives);
        }

        public async Task<RepresentativeResponseDto> GetRepresentativeByNationalIdAsync(string nationalId)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
                throw new ArgumentException("National ID cannot be empty or whitespace.", nameof(nationalId));

            var representative = await _unitOfWork.Representatives.GetByNationalIdAsync(nationalId);
            if (representative == null)
                throw new KeyNotFoundException($"Representative with National ID {nationalId} not found.");

            return _mapper.Map<RepresentativeResponseDto>(representative);
        }
    }

    }