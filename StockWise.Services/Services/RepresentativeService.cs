using AutoMapper;
using Azure;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.RepresentativeDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public async Task<GenericResponse<RepresentativeResponseDto>> CreateRepresentativeAsync(RepresentativeCreateDto Representativedto)
        {
            var respons = new GenericResponse<RepresentativeResponseDto>();
            if (Representativedto == null)
                throw new ArgumentNullException(nameof(Representativedto));

            // Validate WarehouseId
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(Representativedto.WarehouseId);
            if (warehouse == null) 
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Warehouse with ID {Representativedto.WarehouseId} not found.";
                return respons;
            }

            // Validate NationalId uniqueness if provided
            if (!string.IsNullOrWhiteSpace(Representativedto.NationalId))
            {
                var existingRepresentative = await _unitOfWork.Representatives.GetByNationalIdAsync(Representativedto.NationalId);
                if (existingRepresentative != null) 
                {
                    respons.StatusCode = (int)HttpStatusCode.BadRequest;
                    respons.Success = false;
                    respons.Message = $"Representative with National ID {Representativedto.NationalId} already exists.";
                    return respons;
                }
            }

 

            var representative = _mapper.Map<Representative>(Representativedto);
          
            representative.CreatedAt = DateTime.UtcNow;
            representative.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Representatives.AddAsync(representative);
            await _unitOfWork.SaveChangesAsync();


            var createdRepresentative = await _unitOfWork.Representatives.GetByIdAsync(representative.Id);
            respons.StatusCode = (int)HttpStatusCode.Created;
            respons.Success = true;
            respons.Message = "The Representative has been created";
            respons.Data = _mapper.Map<RepresentativeResponseDto>(createdRepresentative); ;
            return respons;
        }

        public async Task<GenericResponse<RepresentativeResponseDto>> DeleteRepresentativeAsync(int id)
        {
            var respons = new GenericResponse<RepresentativeResponseDto>();
            var representative = await _unitOfWork.Representatives.GetByIdAsync(id);
            if (representative == null) {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Representative with ID {id} not found.";
                return respons;
            }

            // Check for related entities
            if (representative.Invoices.Any() || representative.Returns.Any() || representative.Locations.Any() || representative.Expenses.Any())
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Cannot delete representative with associated invoices, returns, locations, or expenses.";
                return respons;
            }
            await _unitOfWork.Representatives.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            respons.StatusCode = (int)HttpStatusCode.NoContent;
            respons.Success = true;
            respons.Message = "The representative has been successfully deleted";
            return respons;
            return respons;
        }

        public async Task<GenericResponse<IEnumerable<RepresentativeResponseDto>>> GetAllRepresentativeAsync()
        {
            var respons =new GenericResponse<IEnumerable<RepresentativeResponseDto>>();
            var representatives = await _unitOfWork.Representatives.GetAllAsync();
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success= true;
            respons.Message = "The operation is successful";
            respons.Data = _mapper.Map<IEnumerable<RepresentativeResponseDto>>(representatives);
            return respons;
        }

        public async Task<GenericResponse<RepresentativeResponseDto>> GetRepresentativeByIdAsync(int id)
        {
            var respons = new GenericResponse<RepresentativeResponseDto>();

            var representative = await _unitOfWork.Representatives.GetByIdAsync(id);
            if (representative == null) 
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Representative with ID {id} not found.";
            }
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "The operation is successful";
            respons.Data = _mapper.Map<RepresentativeResponseDto>(representative);
            return respons ;
        }

        public async Task<GenericResponse<RepresentativeResponseDto>> UpdateRepresentativeAsync(int id, RepresentativeCreateDto representativeDto)
        {
            var respons = new GenericResponse<RepresentativeResponseDto>();
            if (representativeDto == null)
                throw new ArgumentNullException(nameof(representativeDto));

            var existingRepresentative = await _unitOfWork.Representatives.GetByIdAsync(id);
            if (existingRepresentative == null) 
            {
                respons.StatusCode= (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Representative with ID {id} not found.";
                return respons ;    
            }

            // Validate WarehouseId
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(representativeDto.WarehouseId);
            if (warehouse == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Warehouse with ID {representativeDto.WarehouseId} not found.";
                return respons;
            }

            // Validate NationalId uniqueness if provided
            if (!string.IsNullOrWhiteSpace(representativeDto.NationalId))
            {
                var existingByNationalId = await _unitOfWork.Representatives.GetByNationalIdAsync(representativeDto.NationalId);
                if (existingByNationalId != null && existingByNationalId.Id != id) 
                {
                    respons.StatusCode = (int)HttpStatusCode.BadRequest;
                    respons.Success = false;
                    respons.Message = $"Representative with National ID {representativeDto.NationalId} already exists.";
                    return respons;
                }
            }



            _mapper.Map(representativeDto, existingRepresentative);
            existingRepresentative.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Representatives.UpdateAsync(existingRepresentative);
            await _unitOfWork.SaveChangesAsync();
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success=true;
            respons.Message = "The operation is successful";
            respons.Data = _mapper.Map<RepresentativeResponseDto>(existingRepresentative);
            return respons;
        }
        public async Task<GenericResponse<IEnumerable<RepresentativeResponseDto>>> GetRepresentativesByWarehouseIdAsync(int warehouseId)
        {
            var respons =new GenericResponse<IEnumerable<RepresentativeResponseDto>>();
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);
            if (warehouse == null) 
            {
                respons.StatusCode=(int) HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Warehouse with ID {warehouseId} not found.";
                return respons;
            }

            var representatives = await _unitOfWork.Representatives.GetByWarehouseIdAsync(warehouseId);
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "The operation is successful";
            respons.Data = _mapper.Map<IEnumerable<RepresentativeResponseDto>>(representatives);
            return respons;
        }

        public async Task<GenericResponse<RepresentativeResponseDto>> GetRepresentativeByNationalIdAsync(string nationalId)
        {
            var respons = new GenericResponse<RepresentativeResponseDto>();

            if (string.IsNullOrWhiteSpace(nationalId)) 
            {
                respons.StatusCode=(int) HttpStatusCode.BadRequest;
                respons.Success=false;
                respons.Message = "National ID cannot be empty or whitespace.";
                return respons;
            }

            var representative = await _unitOfWork.Representatives.GetByNationalIdAsync(nationalId);
            if (representative == null)
                throw new KeyNotFoundException($"Representative with National ID {nationalId} not found.");

            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "The operation is successful";
            respons.Data = _mapper.Map<RepresentativeResponseDto>(representative);
            return respons;
         
        }
    }

    }