using StockWise.Services.DTOS;
using StockWise.Services.DTOS.RepresentativeDto;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IRepresentativeService
    {
        Task<GenericResponse<IEnumerable<RepresentativeResponseDto>>> GetAllRepresentativeAsync();
        Task<GenericResponse<RepresentativeResponseDto>> GetRepresentativeByIdAsync(int id);
        Task<GenericResponse<RepresentativeResponseDto>> CreateRepresentativeAsync(RepresentativeCreateDto dto);
        Task<GenericResponse<RepresentativeResponseDto>> UpdateRepresentativeAsync(int id,RepresentativeCreateDto dto);
        Task<GenericResponse<IEnumerable<RepresentativeResponseDto>>> GetRepresentativesByWarehouseIdAsync(int warehouseId);
        Task<GenericResponse<RepresentativeResponseDto>> GetRepresentativeByNationalIdAsync(string nationalId);
        Task<GenericResponse<RepresentativeResponseDto>> DeleteRepresentativeAsync(int id);
    }
}
