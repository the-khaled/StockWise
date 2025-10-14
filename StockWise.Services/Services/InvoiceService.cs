using AutoMapper;
using Microsoft.Extensions.Logging;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Domain.ValueObjects;
using StockWise.Services.DTOS.InvoiceDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Services.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<InvoiceService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<InvoiceResponseDto> CreateInvoiceAsync(InvoiceCreateDto invoiceDto)
        {
            try
            {
                _logger.LogInformation("Creating invoice for CustomerId {CustomerId}", invoiceDto.CustomerId);

                if (invoiceDto == null)
                    throw new ArgumentNullException(nameof(invoiceDto));

                if (!invoiceDto.Items.Any())
                    throw new BusinessException("Invoice must contain at least one item.");

                // Validate Customer and Representative
                var customer = await _unitOfWork.Customer.GetByIdAsync(invoiceDto.CustomerId);
                if (customer == null)
                    throw new BusinessException($"Customer with ID {invoiceDto.CustomerId} not found.");

                var representative = await _unitOfWork.Representatives.GetByIdAsync(invoiceDto.RepresentativeId);
                if (representative == null)
                    throw new BusinessException($"Representative with ID {invoiceDto.RepresentativeId} not found.");

                // Calculate TotalAmount automatically
                decimal calculatedTotal = 0;
                var invoiceItems = new List<InvoiceItem>();
                foreach (var itemDto in invoiceDto.Items)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                        throw new BusinessException($"Product with ID {itemDto.ProductId} not found.");

                    var warehouseId = representative.WarehouseId;
                    if (warehouseId <= 0)
                        throw new BusinessException("Representative must have a valid warehouse ID.");

                    var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(itemDto.ProductId, warehouseId);
                    if (stock == null || stock.Quantity < itemDto.Quantity)
                        throw new BusinessException($"Insufficient stock for product ID {itemDto.ProductId}. Available: {stock?.Quantity ?? 0}, Requested: {itemDto.Quantity}");

                    var invoiceItem = new InvoiceItem
                    {
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        Price = new Money(itemDto.Price.Amount, itemDto.Price.Currency ?? "EGP"),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    invoiceItems.Add(invoiceItem);

                    calculatedTotal += itemDto.Quantity * itemDto.Price.Amount;
                }

                // TotalAmount is calculated automatically - ignore user input if provided
                var finalTotalAmount = new Money(calculatedTotal, "EGP");

                // Create Invoice
                var invoice = new Invoice
                {
                    CustomerId = invoiceDto.CustomerId,
                    RepresentativeId = invoiceDto.RepresentativeId,
                    TotalAmount = finalTotalAmount, 
                    Status = invoiceDto.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Items = invoiceItems
                };

                await _unitOfWork.Invoice.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();
                foreach (var item in invoice.Items)
                {
                    item.InvoiceId = invoice.Id; 
                }
                // Update Stock
                foreach (var item in invoice.Items)
                {
                    var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(item.ProductId, representative.WarehouseId);
                    if (stock != null)
                    {
                        stock.Quantity -= item.Quantity;
                        stock.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Stocks.UpdateAsync(stock);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Invoice created with ID {Id}, Total: {Total}", invoice.Id, calculatedTotal);
                var createdInvoice = await _unitOfWork.Invoice.GetByIdWithItemsAsync(invoice.Id);
                return _mapper.Map<InvoiceResponseDto>(createdInvoice);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning("Business error: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                throw;
            }
        }
        public async Task<InvoiceResponseDto> GetInvoiceByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting invoice by ID {Id}", id);
                var invoice = await _unitOfWork.Invoice.GetByIdWithItemsAsync(id);
                if (invoice == null)
                    throw new KeyNotFoundException($"Invoice with ID {id} not found.");

                return _mapper.Map<InvoiceResponseDto>(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync()
        {
            try
            {
                _logger.LogInformation("Getting all invoices");
                var invoices = await _unitOfWork.Invoice.GetAllWithItemsAsync();
                return _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all invoices: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<InvoiceResponseDto> UpdateInvoiceAsync(int id, InvoiceCreateDto invoiceDto)
        {
            try
            {
                _logger.LogInformation("Updating invoice ID {Id}", id);
                if (invoiceDto == null)
                    throw new ArgumentNullException(nameof(invoiceDto));

                var existingInvoice = await _unitOfWork.Invoice.GetByIdWithItemsAsync(id);
                if (existingInvoice == null)
                    throw new KeyNotFoundException($"Invoice with ID {id} not found.");

                if (existingInvoice.Status == InvoiceStatus.Issued || existingInvoice.Status == InvoiceStatus.Paid)
                    throw new BusinessException("Cannot modify an invoice that is Issued or Paid.");

                var customer = await _unitOfWork.Customer.GetByIdAsync(invoiceDto.CustomerId);
                if (customer == null)
                    throw new BusinessException($"Customer with ID {invoiceDto.CustomerId} not found.");

                var representative = await _unitOfWork.Representatives.GetByIdAsync(invoiceDto.RepresentativeId);
                if (representative == null)
                    throw new BusinessException($"Representative with ID {invoiceDto.RepresentativeId} not found.");

                // Calculate new total
                decimal calculatedTotal = 0;
                foreach (var item in invoiceDto.Items)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product == null)
                        throw new BusinessException($"Product with ID {item.ProductId} not found.");

                    var warehouseId = representative.WarehouseId > 0 ? representative.WarehouseId : 1;
                    var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(item.ProductId, warehouseId);
                    if (stock == null || stock.Quantity < item.Quantity)
                        throw new BusinessException($"Insufficient stock for product ID {item.ProductId}. Available: {stock?.Quantity ?? 0}, Requested: {item.Quantity}");

                    calculatedTotal += item.Quantity * item.Price.Amount;
                }

                // Update basic invoice properties
                existingInvoice.CustomerId = invoiceDto.CustomerId;
                existingInvoice.RepresentativeId = invoiceDto.RepresentativeId;
                existingInvoice.TotalAmount = new Money(calculatedTotal, "EGP");
                existingInvoice.UpdatedAt = DateTime.UtcNow;

                // Delete existing InvoiceItems
                var existingItems = (await _unitOfWork.InvoiceItem.GetAllAsync())
                    .Where(ii => ii.InvoiceId == id)
                    .ToList();
                foreach (var item in existingItems)
                {
                    _unitOfWork.InvoiceItem.Remove(item);
                }

                // Add new InvoiceItems
                foreach (var itemDto in invoiceDto.Items)
                {
                    var newItem = new InvoiceItem
                    {
                        InvoiceId = id,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        Price = new Money(itemDto.Price.Amount, itemDto.Price.Currency ?? "EGP"),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.InvoiceItem.AddAsync(newItem); 
                }

                // Update Stock for new items 
                foreach (var itemDto in invoiceDto.Items)
                {
                    var warehouseId = representative.WarehouseId > 0 ? representative.WarehouseId : 1;
                    var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(itemDto.ProductId, warehouseId);
                    if (stock != null)
                    {
                        stock.Quantity -= itemDto.Quantity;
                        stock.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Stocks.UpdateAsync(stock); 
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Invoice ID {Id} updated successfully", id);

                // Return updated invoice
                var updatedInvoice = await _unitOfWork.Invoice.GetByIdWithItemsAsync(id);
                return _mapper.Map<InvoiceResponseDto>(updatedInvoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task DeleteInvoiceAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting invoice ID {Id}", id);
                var invoice = await _unitOfWork.Invoice.GetByIdAsync(id);
                if (invoice == null)
                    throw new KeyNotFoundException($"Invoice with ID {id} not found.");

                if (invoice.Status == InvoiceStatus.Issued || invoice.Status == InvoiceStatus.Paid)
                    throw new BusinessException("Cannot delete an invoice that is Issued or Paid.");

                // Delete InvoiceItems 
                var items = (await _unitOfWork.InvoiceItem.GetAllAsync())
                    .Where(ii => ii.InvoiceId == id)
                    .ToList();
                foreach (var item in items)
                {
                    _unitOfWork.InvoiceItem.Remove(item); 
                }

                await _unitOfWork.Invoice.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Invoice ID {Id} deleted successfully", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }
    }
}
