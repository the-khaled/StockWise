using Microsoft.AspNetCore.Http.HttpResults;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
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
        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreatCastomerAsync(CustomerDto customerDto)
        {
           if (customerDto == null)
                throw new ArgumentNullException(nameof(customerDto));
            if (customerDto.CreditBalance != null && customerDto.CreditBalance.Amount < 0)
                throw new BusinessException("Credit balance cannot be negative.");
            var customer = MapToEntity(customerDto);
            await _unitOfWork.Customer.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(int id)
        {
            var existingCustomer = await _unitOfWork.Customer.GetByIdAsync(id);
            if (existingCustomer == null)
                throw new KeyNotFoundException($"Customer with ID {id} not found.");
            await _unitOfWork.Customer.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var Customers= await _unitOfWork.Customer.GetAllAsync();
            return Customers.Select(x =>MapToDto(x)).ToList();
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int id)
        {
            var customer = await _unitOfWork.Customer.GetByIdAsync(id);
            if (customer == null) throw new KeyNotFoundException($"Customer with ID {id} not found.");
            return MapToDto(customer);
        }

        public async Task UpdateCustomerAsync(CustomerDto customerdto)
        {
            if (customerdto == null)
                throw new ArgumentNullException(nameof(customerdto));
            var existingCustomer = await _unitOfWork.Customer.GetByIdAsync(customerdto.Id);
            if (existingCustomer == null)
                throw new KeyNotFoundException($"Customer with ID {customerdto.Id} not found.");
            if (customerdto.CreditBalance != null && customerdto.CreditBalance.Amount < 0)
                throw new BusinessException("Credit balance cannot be negative.");
            existingCustomer.Name = customerdto.Name;
            existingCustomer.PhoneNumbers = customerdto.PhoneNumbers;
            existingCustomer.Address = customerdto.Address;
            existingCustomer.CreditBalance = customerdto.CreditBalance;
            existingCustomer.UpdatedAt = DateTime.Now;
            await _unitOfWork.Customer.UpdateAsync(existingCustomer);
            await _unitOfWork.SaveChangesAsync();
        }
        private CustomerDto MapToDto(Customer customer) 
        {
            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumbers=customer.PhoneNumbers.ToList(),
                Address =customer.Address,
                CreditBalance=customer.CreditBalance,
                UpdatedAt = customer.UpdatedAt,
                CreatedAt = customer.CreatedAt

            };
        }
        private Customer MapToEntity(CustomerDto customerdto) 
        {
            return new Customer
            {
                Id = customerdto.Id,
                Name = customerdto.Name,
                PhoneNumbers = customerdto.PhoneNumbers.ToList(),
                Address = customerdto.Address,
                CreditBalance = customerdto.CreditBalance,
                UpdatedAt = customerdto.UpdatedAt,
                CreatedAt = customerdto.CreatedAt
            };
        }
    }
}
