using StockWise.Services.DTOS;
using StockWise.Services.DTOS.RepresentativeDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IRepresentativeService
    {
        Task<IEnumerable<RepresentativeResponseDto>> GetAllRepresentativeAsync();
        Task<RepresentativeResponseDto> GetRepresentativeByIdAsync(int id);
        Task<RepresentativeResponseDto> CreateRepresentativeAsync(RepresentativeCreateDto dto);
        Task<RepresentativeResponseDto> UpdateRepresentativeAsync(int id,RepresentativeCreateDto dto);
        Task<IEnumerable<RepresentativeResponseDto>> GetRepresentativesByWarehouseIdAsync(int warehouseId);
        Task<RepresentativeResponseDto> GetRepresentativeByNationalIdAsync(string nationalId);
        Task DeleteRepresentativeAsync(int id);
    }
}
