using StockWise.Services.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IRepresentativeService
    {
        Task<IEnumerable<RepresentativeDto>> GetAllRepresentativeAsync();
        Task<RepresentativeDto> GetRepresentativeByIdAsync(int id);
        Task CreateRepresentativeAsync(RepresentativeDto dto);
        Task UpdateRepresentativeAsync(RepresentativeDto dto);
        Task DeleteRepresentativeAsync(int id);
    }
}
