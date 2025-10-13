using Microsoft.AspNetCore.Http.HttpResults;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class InvoiceItemService : IInvoiceItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        public InvoiceItemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreateInvoiceItemAsync(InvoiceItemDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Quantity <= 0)
                throw new BusinessException("Quantity must be greater than zero.");

            if (dto.Price.Amount < 0)
                throw new BusinessException("Price cannot be negative.");

            var invoice = await _unitOfWork.Invoice.GetByIdAsync(dto.InvoiceId);
            if (invoice == null)
                throw new BusinessException("Invoice not found.");

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new BusinessException("Product not found.");

            var invoiceItem = MapToEntity(dto);
            await _unitOfWork.InvoiceItem.AddAsync(invoiceItem);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteInvoiceItemAsync(int id)
        {
            var invoiceItem = await _unitOfWork.InvoiceItem.GetByIdAsync(id);
            if (invoiceItem == null)
                throw new KeyNotFoundException($"Invoice item with ID {id} not found.");
            await _unitOfWork.InvoiceItem.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<InvoiceItemDto>> GetAllInvoiceItemAsync()
        {
            var invoiceItems = await _unitOfWork.InvoiceItem.GetAllAsync();
            return invoiceItems.Select(i => MapToDto(i)).ToList();
        }

        public async Task<InvoiceItemDto> GetInvoiceItemByIdAsync(int id)
        {
            var invoiceItem = await _unitOfWork.InvoiceItem.GetByIdAsync(id);
            if (invoiceItem == null)
                throw new KeyNotFoundException($"Invoice item with ID {id} not found.");
            return MapToDto(invoiceItem);
        }

        public async Task UpdateInvoiceItemAsync(InvoiceItemDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var existingInvoiceItem = await _unitOfWork.InvoiceItem.GetByIdAsync(dto.Id);
            if (existingInvoiceItem == null)
                throw new KeyNotFoundException($"Invoice item with ID {dto.Id} not found.");
            var invoice = await _unitOfWork.Invoice.GetByIdAsync(dto.InvoiceId);
            if (invoice == null)
                throw new BusinessException("Invoice not found.");

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new BusinessException("Product not found.");

            existingInvoiceItem.InvoiceId = dto.InvoiceId;
            existingInvoiceItem.ProductId = dto.ProductId;
            existingInvoiceItem.Quantity = dto.Quantity;
            existingInvoiceItem.Price = dto.Price;
            existingInvoiceItem.UpdatedAt = DateTime.Now;

            await _unitOfWork.InvoiceItem.UpdateAsync(existingInvoiceItem);
            await _unitOfWork.SaveChangesAsync();
        }
        public InvoiceItem MapToEntity(InvoiceItemDto InvoiceItemdto) 
        {
            return new InvoiceItem 
            {
                Id = InvoiceItemdto.Id,
                InvoiceId = InvoiceItemdto.InvoiceId,
                ProductId = InvoiceItemdto.ProductId,
                Quantity = InvoiceItemdto.Quantity,
                Price = InvoiceItemdto.Price,
             
            };
        }
        public InvoiceItemDto MapToDto(InvoiceItem invoiceItem) 
        {
            return new InvoiceItemDto
            { 
                Id = invoiceItem.Id,
                InvoiceId = invoiceItem.InvoiceId,
                ProductId = invoiceItem.ProductId,
                Quantity = invoiceItem.Quantity,
                Price = invoiceItem.Price,
               

            };
        }
    }
}