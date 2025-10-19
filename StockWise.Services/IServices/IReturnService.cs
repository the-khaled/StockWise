using StockWise.Services.DTOS;
using StockWise.Services.DTOS.ReturnDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IReturnService
    {
        Task<IEnumerable<ReturnResponseDto>> GetAllReturnAsync();
        Task<ReturnResponseDto> GetReturnsByIdAsync(int id);
        Task<ReturnResponseDto> UpdateReturnAsync(int id,ReturnCreateDto ReturnDto);
        Task<ReturnResponseDto> CreateReturnAsync(ReturnCreateDto ReturnDto);
        Task<IEnumerable<ReturnResponseDto>> GetReturnsByProductIdAsync(int productId);
        Task<IEnumerable<ReturnResponseDto>> GetReturnsByRepresentativeIdAsync(int representativeId);
        Task<IEnumerable<ReturnResponseDto>> GetReturnsByCustomerIdAsync(int customerId);
        Task DeleteReturnAsync(int id);
    }
}
