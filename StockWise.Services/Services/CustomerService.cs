using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http.HttpResults;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.CustomerDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper=mapper;
        }
        public async Task<GenericResponse<CustomerResponseDto>> CreatCastomerAsync(CustomerCreateDto customerDto)
        {
            var responce = new GenericResponse<CustomerResponseDto>();

            if (customerDto == null)
            {                
                responce.StatusCode = (int)HttpStatusCode.BadRequest;
                responce.Success = false ;
                responce.Message = "Customer data is required.";
                responce.Data = null;
                return responce;
            }
            if (customerDto.CreditBalance != null && customerDto.CreditBalance.Amount < 0)
            {
                responce.StatusCode = (int)HttpStatusCode.BadRequest;
                responce.Success = false;
                responce.Message = "Credit balance cannot be negative.";
                responce.Data = null;
                return responce;
         }
            var allowedCurrencies = new[] { "EGP", "$", "USD", "EUR" };
            customerDto.CreditBalance.Currency = customerDto.CreditBalance.Currency?.Trim();

            if (string.IsNullOrWhiteSpace(customerDto.CreditBalance.Currency) || !allowedCurrencies.Contains(customerDto.CreditBalance.Currency))
            {
                responce.StatusCode = (int)HttpStatusCode.BadRequest;
                responce.Success = false;
                responce.Message = "Invalid currency , Allowed values are: EGP , $ , USD , EUR ";
                responce.Data = null;
                return responce;
            }
     /*       var existingCustomer = await _unitOfWork.Customer.GetByNameAsync(customerDto.Name);
            if (existingCustomer.Any())
            {
                responce.StatusCode = (int)HttpStatusCode.BadRequest;
                responce.Success = false;
                responce.Message = $"Customer with name '{customerDto.Name}' already exists.";
                responce.Data = null;
                return responce;
            }*/

            var Customer = _mapper.Map<Customer>(customerDto);
            Customer.CreatedAt = DateTime.UtcNow;
            Customer.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Customer.AddAsync(Customer);
            await _unitOfWork.SaveChangesAsync();
            var createdCustomer = await _unitOfWork.Customer.GetByIdAsync(Customer.Id);

            responce.Data = _mapper.Map<CustomerResponseDto>(createdCustomer);
            responce.StatusCode =(int)HttpStatusCode.Created;
            responce.Success = true;
            responce.Message = "Successful operation";
            return responce;

        }

        public async Task<GenericResponse<CustomerResponseDto>> DeleteCustomerAsync(int id)
        {
            var responce = new GenericResponse<CustomerResponseDto>();
            var Customer = await _unitOfWork.Customer.GetByIdAsync(id);
            if (Customer == null)
            {
                responce.StatusCode = (int)HttpStatusCode.BadRequest;
                responce.Success = false;
                responce.Message = $"Customer with ID {id} not found.";
                responce.Data = null;
                return responce;
               // throw new KeyNotFoundException($"Customer with ID {id} not found.");
            }
            if (Customer.Invoices.Any() || Customer.Payments.Any() || Customer.Returns.Any())
            {
                responce.StatusCode = (int)HttpStatusCode.NoContent;
                responce.Success = false;
                responce.Message = "Cannot delete customer with associated invoices, payments, or returns.";
                return responce ;
               // throw new BusinessException("Cannot delete customer with associated invoices, payments, or returns.");
            }
            await _unitOfWork.Customer.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            responce.StatusCode = (int)HttpStatusCode.OK;
            responce.Success = true;
            responce.Message = "Customer has been deleted successfully.";
            responce.Data = null;
            return responce;
        }

        public async Task<GenericResponse<IEnumerable<CustomerResponseDto>>> GetAllCustomersAsync()
        {
            var responce = new GenericResponse<IEnumerable<CustomerResponseDto>>();
            var Customers= await _unitOfWork.Customer.GetAllAsync();
            responce.StatusCode = (int)HttpStatusCode.OK;
            responce.Success = true;
            responce.Message = "Successful operation";
            responce.Data = _mapper.Map<IEnumerable<CustomerResponseDto>>(Customers);

            return responce;
        }

        public async Task<GenericResponse<CustomerResponseDto>> GetCustomerByIdAsync(int id)
        {
            var responce=new GenericResponse<CustomerResponseDto>();
            var customer = await _unitOfWork.Customer.GetByIdAsync(id);
            if (customer == null)
            {
                responce.Data =null;
                responce.StatusCode = (int)HttpStatusCode.NotFound;
                responce.Success = false;
                responce.Message = $"Customer with ID {id} not found.";
                return responce;
                //throw new KeyNotFoundException($"Customer with ID {id} not found.");
            }
            responce.Data = _mapper.Map<CustomerResponseDto>(customer);
            responce.StatusCode = 200;
            responce.Success=true;
            responce.Message = "Successful operation";
            return responce;
        }

        public async Task<GenericResponse<CustomerResponseDto>> UpdateCustomerAsync(int id,CustomerCreateDto customerdto)
        {
            var respons = new GenericResponse<CustomerResponseDto>();
            if (customerdto == null)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Customer information is required.";
                respons.Data = null;
                return respons;
            }
            var existingCustomer = await _unitOfWork.Customer.GetByIdAsync(id);
            if (existingCustomer == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Customer with ID {id} not found.";
                respons.Data = null;
                return respons;
            }
            if (customerdto.CreditBalance != null && customerdto.CreditBalance.Amount < 0)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Credit balance cannot be negative.";
                respons.Data = null;
                return respons;
            }
            customerdto.CreditBalance.Currency = customerdto.CreditBalance.Currency?.Trim();
            var allowedCurrencies = new[] { "EGP", "$", "USD", "EUR" };
            if (string.IsNullOrWhiteSpace(customerdto.CreditBalance.Currency) || !allowedCurrencies.Contains(customerdto.CreditBalance.Currency))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Invalid currency";
                respons.Data = null;
                return respons;
            }
   /*         var duplicateCustomer = await _unitOfWork.Customer.GetByNameAsync(customerdto.Name);
            if (duplicateCustomer.Any(c => c.Id != id))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = $"Customer with name '{customerdto.Name}' already exists.";
                respons.Data = null;
                return respons;
               // throw new BusinessException($"Customer with name '{customerdto.Name}' already exists.");
            }
*/
            _mapper.Map(customerdto, existingCustomer);
            existingCustomer.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Customer.UpdateAsync(existingCustomer);
            await _unitOfWork.SaveChangesAsync();

            respons.StatusCode = (int)HttpStatusCode.NoContent;
            respons.Success = true;
            respons.Message = "Successful operation.";
            respons.Data = _mapper.Map<CustomerResponseDto>(existingCustomer);
            return respons;
          
        }

        public async Task<GenericResponse<IEnumerable<CustomerResponseDto>>> GetCustomersByNameAsync(string name)
        {
            var responce = new GenericResponse<IEnumerable<CustomerResponseDto>>();
            if (string.IsNullOrWhiteSpace(name))
            {
                responce.StatusCode=(int)HttpStatusCode.BadRequest;
                responce.Message= "Name cannot be empty or whitespace.";
                responce.Success = false;
                responce.Data = null;
                return responce;
                //throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));
            }
            var customers = await _unitOfWork.Customer.GetByNameAsync(name);
            if (customers == null || !customers.Any()) 
            {
                responce.StatusCode = (int)HttpStatusCode.NotFound;
                responce.Message = $"Customer with Name {name} not found."; 
                responce.Success = false;
                responce.Data = null;
                return responce;
            }
            responce.StatusCode = (int)HttpStatusCode.OK;
            responce.Message = "Successful operation";
            responce.Success = true;
            responce.Data = _mapper.Map<IEnumerable<CustomerResponseDto>>(customers);
            return responce;
        }

    }
}
