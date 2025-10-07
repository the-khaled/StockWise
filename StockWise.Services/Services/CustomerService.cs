using StockWise.Services.DTOS;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class CustomerService : ICustomerService
    {
        public Task CreatCastomerAsync(CustomerDto customerDto)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCustomerAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CustomerDto> GetCustomerByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCustomerAsync(CustomerDto customerto)
        {
            throw new NotImplementedException();
        }
    }
}
