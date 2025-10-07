using StockWise.Services.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto> GetCustomerByIdAsync(int id);
        Task UpdateCustomerAsync(CustomerDto customerto);
        Task CreatCastomerAsync(CustomerDto customerDto);
        Task DeleteCustomerAsync(int id);
    }
}
