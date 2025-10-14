using AutoMapper;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Domain.ValueObjects;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.InvoiceItemDto.InvoiceItemDto;
using StockWise.Services.DTOS.InvoiceItemDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace StockWise.Services.Services
{
    public class InvoiceItemService : IInvoiceItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InvoiceItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<InvoiceItemResponseDto> CreateInvoiceItemAsync(InvoiceItemCreateDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (!createDto.InvoiceId.HasValue || createDto.InvoiceId <= 0)
                throw new BusinessException("Valid InvoiceId is required.");

            if (createDto.Quantity <= 0)
                throw new BusinessException("Quantity must be greater than zero.");

            if (createDto.Price.Amount < 0)
                throw new BusinessException("Price cannot be negative.");

            var invoice = await _unitOfWork.Invoice.GetByIdAsync(createDto.InvoiceId.Value);
            if (invoice == null)
                throw new BusinessException("Invoice not found.");

            var representative = await _unitOfWork.Representatives.GetByIdAsync(invoice.RepresentativeId);
            if (representative == null)
                throw new BusinessException("Representative not found.");

            var product = await _unitOfWork.Products.GetByIdAsync(createDto.ProductId);
            if (product == null)
                throw new BusinessException("Product not found.");

            //  Stock validation
            var warehouseId = representative.WarehouseId ;
            var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(createDto.ProductId, warehouseId);
            if (stock == null || stock.Quantity < createDto.Quantity)
                throw new BusinessException($"Insufficient stock for product ID {createDto.ProductId}. Available: {stock?.Quantity ?? 0}, Requested: {createDto.Quantity}");

            var invoiceItem = new InvoiceItem
            {
                InvoiceId = createDto.InvoiceId.Value,
                ProductId = createDto.ProductId,
                Quantity = createDto.Quantity,
                Price = createDto.Price!=null? new Money(createDto.Price.Amount, createDto.Price.Currency ?? "EGP")
            : product.Price,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.InvoiceItem.AddAsync(invoiceItem);
            await _unitOfWork.SaveChangesAsync();

            // Update Stock
            if (stock != null)
            {
                stock.Quantity -= createDto.Quantity;
                stock.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Stocks.UpdateAsync(stock);
            }
            await _unitOfWork.SaveChangesAsync();

            var createdItem = await _unitOfWork.InvoiceItem.GetByIdAsync(invoiceItem.Id);
            return _mapper.Map<InvoiceItemResponseDto>(createdItem);
        }

        public async Task DeleteInvoiceItemAsync(int id)
        {
            var invoiceItem = await _unitOfWork.InvoiceItem.GetByIdAsync(id);
            if (invoiceItem == null)
                throw new KeyNotFoundException($"Invoice item with ID {id} not found.");

            //  Restore Stock
            var invoice = await _unitOfWork.Invoice.GetByIdAsync(invoiceItem.InvoiceId);
            if (invoice != null)
            {
                var representative = await _unitOfWork.Representatives.GetByIdAsync(invoice.RepresentativeId);
                var warehouseId = representative?.WarehouseId ?? 1;
                var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(invoiceItem.ProductId, warehouseId);
                if (stock != null)
                {
                    stock.Quantity += invoiceItem.Quantity; //  Restore quantity
                    stock.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Stocks.UpdateAsync(stock);
                }
            }

            await _unitOfWork.InvoiceItem.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<InvoiceItemResponseDto>> GetAllInvoiceItemAsync()
        {
            var invoiceItems = await _unitOfWork.InvoiceItem.GetAllAsync();
            return _mapper.Map<IEnumerable<InvoiceItemResponseDto>>(invoiceItems);
        }

        public async Task<InvoiceItemResponseDto> GetInvoiceItemByIdAsync(int id)
        {
            var invoiceItem = await _unitOfWork.InvoiceItem.GetByIdAsync(id);
            if (invoiceItem == null)
                throw new KeyNotFoundException($"Invoice item with ID {id} not found.");
            return _mapper.Map<InvoiceItemResponseDto>(invoiceItem);
        }

        public async Task<InvoiceItemResponseDto> UpdateInvoiceItemAsync(int id, InvoiceItemCreateDto updateDto)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            if (!updateDto.InvoiceId.HasValue || updateDto.InvoiceId <= 0)
                throw new BusinessException("Valid InvoiceId is required.");

            var existingInvoiceItem = await _unitOfWork.InvoiceItem.GetByIdAsync(id);
            if (existingInvoiceItem == null)
                throw new KeyNotFoundException($"Invoice item with ID {id} not found.");

            var invoice = await _unitOfWork.Invoice.GetByIdAsync(updateDto.InvoiceId.Value);
            if (invoice == null)
                throw new BusinessException("Invoice not found.");

            var representative = await _unitOfWork.Representatives.GetByIdAsync(invoice.RepresentativeId);
            if (representative == null)
                throw new BusinessException("Representative not found.");

            var product = await _unitOfWork.Products.GetByIdAsync(updateDto.ProductId);
            if (product == null)
                throw new BusinessException("Product not found.");

            //  Stock validation (مع الـ old quantity)
            var warehouseId = representative.WarehouseId ;
            var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(updateDto.ProductId, warehouseId);
            if (stock == null || stock.Quantity + existingInvoiceItem.Quantity < updateDto.Quantity)
                throw new BusinessException($"Insufficient stock for product ID {updateDto.ProductId}. Available: {stock?.Quantity + existingInvoiceItem.Quantity ?? 0}, Requested: {updateDto.Quantity}");

            //  Restore old quantity to stock
            if (stock != null && existingInvoiceItem.ProductId == updateDto.ProductId)
            {
                stock.Quantity += existingInvoiceItem.Quantity; // Restore
                stock.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Stocks.UpdateAsync(stock);
            }

            // Update properties
            existingInvoiceItem.InvoiceId = updateDto.InvoiceId.Value;
            existingInvoiceItem.ProductId = updateDto.ProductId;
            existingInvoiceItem.Quantity = updateDto.Quantity;
            existingInvoiceItem.Price = updateDto.Price != null
        ? new Money(updateDto.Price.Amount, updateDto.Price.Currency ?? "EGP")
        : product.Price;
            existingInvoiceItem.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.InvoiceItem.UpdateAsync(existingInvoiceItem);

            // Deduct new quantity
            if (stock != null)
            {
                stock.Quantity -= updateDto.Quantity;
                stock.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Stocks.UpdateAsync(stock);
            }

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<InvoiceItemResponseDto>(existingInvoiceItem);
        }
    }
}