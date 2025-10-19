using StockWise.Services.DTOS;
using StockWise.Services.DTOS.CustomerDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerResponseDto>> GetAllCustomersAsync();
        Task<CustomerResponseDto> GetCustomerByIdAsync(int id);
        Task<CustomerResponseDto> UpdateCustomerAsync(int id,CustomerCreateDto customerto);
        Task <CustomerResponseDto>CreatCastomerAsync(CustomerCreateDto customerDto);
        Task<IEnumerable<CustomerResponseDto>> GetCustomersByNameAsync(string name);
        Task DeleteCustomerAsync(int id);
    }
}
