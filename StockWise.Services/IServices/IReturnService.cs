using StockWise.Services.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IReturnService
    {
        Task<IEnumerable<ReturnDto>> GetAllReturnAsync();
        Task<ReturnDto> GetReturnsByIdAsync(int id);
        Task UpdateReturnAsync(ReturnDto ReturnDto);
        Task CreateReturnAsync(ReturnDto ReturnDto);
        Task DeleteReturnAsync(int id);
    }
}
