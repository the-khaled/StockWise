using AutoMapper;
using Microsoft.Extensions.Logging;
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
    public class LocationService: ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public LocationService(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
          
        }

        public async Task<IEnumerable<LocationDto>> GetAllLocationsAsync()
        {
            var locations = await _unitOfWork.Location.GetAllAsync();
            return locations.Select(l => MapToDto(l)).ToList();
        }

        public async Task<LocationDto> GetLocationByIdAsync(int id)
        {
            var location = await _unitOfWork.Location.GetByIdAsync(id);
            if (location == null)
                throw new KeyNotFoundException($"Location with ID {id} not found.");
            return MapToDto(location);
        }

        public async Task CreateLocationAsync(LocationDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var representative = await _unitOfWork.Representatives.GetByIdAsync(dto.RepresentativeId);
            if (representative == null)
                throw new BusinessException("Representative not found.");

            await _unitOfWork.Location.AddAsync(MapToEntity(dto));
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateLocationAsync(LocationDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var existingLocation = await _unitOfWork.Location.GetByIdAsync(dto.Id);
            if (existingLocation == null)
                throw new KeyNotFoundException($"Location with ID {dto.Id} not found.");

            var representative = await _unitOfWork.Representatives.GetByIdAsync(dto.RepresentativeId);
            if (representative == null)
                throw new BusinessException("Representative not found.");

            existingLocation.Latitude = dto.Latitude;
            existingLocation.Longitude = dto.Longitude;
            existingLocation.RepresentativeId = dto.RepresentativeId;
            existingLocation.UpdatedAt = DateTime.Now;

            await _unitOfWork.Location.UpdateAsync(existingLocation);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteLocationAsync(int id)
        {
            var location = await _unitOfWork.Location.GetByIdAsync(id);
            if (location == null)
                throw new KeyNotFoundException($"Location with ID {id} not found.");

            await _unitOfWork.Location.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        private LocationDto MapToDto(Location location)
        {
            return new LocationDto
            {
                Id = location.Id,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                RepresentativeId = location.RepresentativeId,
                CreatedAt = location.CreatedAt,
                UpdatedAt = location.UpdatedAt
            };
        }

        private Location MapToEntity(LocationDto dto)
        {
            return new Location
            {
                Id = dto.Id,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                RepresentativeId = dto.RepresentativeId,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }
}

