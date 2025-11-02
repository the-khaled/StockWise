using StockWise.Services.DTOS;
using StockWise.Services.DTOS.CustomerDto;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface ICustomerService
    {
        Task<GenericResponse<IEnumerable<CustomerResponseDto>>> GetAllCustomersAsync();
        Task<GenericResponse<CustomerResponseDto>> GetCustomerByIdAsync(int id);
        Task<GenericResponse<CustomerResponseDto>> UpdateCustomerAsync(int id,CustomerCreateDto customerto);
        Task<GenericResponse<CustomerResponseDto>> CreatCastomerAsync(CustomerCreateDto customerDto);
        Task<GenericResponse<IEnumerable<CustomerResponseDto>>> GetCustomersByNameAsync(string name);
        Task<GenericResponse<CustomerResponseDto>> DeleteCustomerAsync(int id);
    }
}
