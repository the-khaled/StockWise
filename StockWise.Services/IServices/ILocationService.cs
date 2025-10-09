using StockWise.Services.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface ILocationService
    {
        Task<IEnumerable<LocationDto>> GetAllLocationsAsync();
        Task<LocationDto> GetLocationByIdAsync(int id);
        Task CreateLocationAsync(LocationDto dto);
        Task UpdateLocationAsync(LocationDto dto);
        Task DeleteLocationAsync(int id);
    }
}
