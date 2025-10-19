using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.CustomerDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
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
        public async Task<CustomerResponseDto> CreatCastomerAsync(CustomerCreateDto customerDto)
        {
           if (customerDto == null)
                throw new ArgumentNullException(nameof(customerDto));
            if (customerDto.CreditBalance != null && customerDto.CreditBalance.Amount < 0)
                throw new BusinessException("Credit balance cannot be negative.");
            var existingCustomer = await _unitOfWork.Customer.GetByNameAsync(customerDto.Name);
            if (existingCustomer.Any())
                throw new BusinessException($"Customer with name '{customerDto.Name}' already exists.");

            var Customer = _mapper.Map<Customer>(customerDto);
            Customer.CreatedAt = DateTime.UtcNow;
            Customer.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Customer.AddAsync(Customer);
            await _unitOfWork.SaveChangesAsync();
            var createdCustomer = await _unitOfWork.Customer.GetByIdAsync(Customer.Id);
            return _mapper.Map<CustomerResponseDto>(createdCustomer);

        }

        public async Task DeleteCustomerAsync(int id)
        {
            var Customer = await _unitOfWork.Customer.GetByIdAsync(id);
            if (Customer == null)
                throw new KeyNotFoundException($"Customer with ID {id} not found.");
            if (Customer.Invoices.Any() || Customer.Payments.Any() || Customer.Returns.Any())
                throw new BusinessException("Cannot delete customer with associated invoices, payments, or returns.");
            await _unitOfWork.Customer.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<CustomerResponseDto>> GetAllCustomersAsync()
        {
            var Customers= await _unitOfWork.Customer.GetAllAsync();     
            return _mapper.Map<IEnumerable<CustomerResponseDto>>(Customers);
        }

        public async Task<CustomerResponseDto> GetCustomerByIdAsync(int id)
        {
            var customer = await _unitOfWork.Customer.GetByIdAsync(id);
            if (customer == null) throw new KeyNotFoundException($"Customer with ID {id} not found.");
            return _mapper.Map<CustomerResponseDto>(customer);
        }

        public async Task<CustomerResponseDto> UpdateCustomerAsync(int id,CustomerCreateDto customerdto)
        {
            if (customerdto == null)
                throw new ArgumentNullException(nameof(customerdto));
            var existingCustomer = await _unitOfWork.Customer.GetByIdAsync(id);
            if (existingCustomer == null)
                throw new KeyNotFoundException($"Customer with ID {id} not found.");
            if (customerdto.CreditBalance != null && customerdto.CreditBalance.Amount < 0)
                throw new BusinessException("Credit balance cannot be negative.");
            var duplicateCustomer = await _unitOfWork.Customer.GetByNameAsync(customerdto.Name);
            if (duplicateCustomer.Any(c => c.Id != id))
                throw new BusinessException($"Customer with name '{customerdto.Name}' already exists.");

            _mapper.Map(customerdto, existingCustomer);
            existingCustomer.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Customer.UpdateAsync(existingCustomer);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CustomerResponseDto>(existingCustomer);
          
        }

        public async Task<IEnumerable<CustomerResponseDto>> GetCustomersByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));
            var customers = await _unitOfWork.Customer.GetByNameAsync(name);
            return _mapper.Map<IEnumerable<CustomerResponseDto>>(customers);
        }

    }
}
