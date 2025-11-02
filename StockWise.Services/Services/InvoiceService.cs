using AutoMapper;
using Azure;
using Microsoft.Extensions.Logging;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Domain.ValueObjects;
using StockWise.Services.DTOS.InvoiceDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<InvoiceService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
          
        }

        public async Task<GenericResponse<InvoiceResponseDto>> CreateInvoiceAsync(InvoiceCreateDto invoiceDto)
        {
            try
            {
                var respons=new GenericResponse<InvoiceResponseDto>();
                if (invoiceDto == null)
                {
                    respons.StatusCode=(int)HttpStatusCode.BadRequest;
                    respons.Success=false;
                    respons.Message = "Invoice data is required.";
                    return respons;
                }

                if (!invoiceDto.Items.Any())
                {
                    respons.StatusCode = (int)HttpStatusCode.BadRequest;
                    respons.Success = false;
                    respons.Message = "Invoice must contain at least one item.";
                    return respons;
                }

                // Validate Customer and Representative
                var customer = await _unitOfWork.Customer.GetByIdAsync(invoiceDto.CustomerId);
                if (customer == null)
                {
                    respons.StatusCode = (int)HttpStatusCode.NotFound;
                    respons.Success = false;
                    respons.Message = $"Customer with ID {invoiceDto.CustomerId} not found.";
                    return respons;
                }

                var representative = await _unitOfWork.Representatives.GetByIdAsync(invoiceDto.RepresentativeId);
                if (representative == null)
                {
                    respons.StatusCode = (int)HttpStatusCode.NotFound;
                    respons.Success = false;
                    respons.Message = $"Representative with ID {invoiceDto.RepresentativeId} not found.";
                    return respons;
                }

                // Calculate TotalAmount automatically
                decimal calculatedTotal = 0;
                var invoiceItems = new List<InvoiceItem>();
                var currency = "EGP";
                foreach (var itemDto in invoiceDto.Items)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        respons.StatusCode = (int)HttpStatusCode.NotFound;
                        respons.Success = false;
                        respons.Message = $"Product with ID {itemDto.ProductId} not found.";
                        return respons;
                    }

                    var warehouseId = representative.WarehouseId;
                    if (warehouseId <= 0)
                    {
                        respons.StatusCode = (int)HttpStatusCode.NotFound;
                        respons.Success = false;
                        respons.Message = "Representative must have a valid warehouse ID.";
                        return respons;
                    }

                    var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(itemDto.ProductId, warehouseId);
                    if (stock == null || stock.Quantity < itemDto.Quantity)
                    {
                        respons.StatusCode = (int)HttpStatusCode.BadRequest;
                        respons.Success = false;
                        respons.Message =
                            $"Insufficient stock for product ID {itemDto.ProductId}. Available: {stock?.Quantity ?? 0}, Requested: {itemDto.Quantity}";
                        return respons;
                    }
                    var allowedCurrencies = new[] { "EGP", "$", "USD", "EUR" };
                    itemDto.Price.Currency = itemDto.Price.Currency?.Trim();
                    if (string.IsNullOrWhiteSpace(itemDto.Price.Currency) || !allowedCurrencies.Contains(itemDto.Price.Currency))
                    {
                        respons.StatusCode = (int)HttpStatusCode.BadRequest;
                        respons.Success = false;
                        respons.Message = "Invalid currency , Allowed values are: EGP , $ , USD , EUR ";
                        respons.Data = null;
                        return respons;
                    }
                    currency= itemDto.Price.Currency;
                    if (itemDto.Price == null || itemDto.Price.Amount < 0)
                    {
                        respons.StatusCode = (int)HttpStatusCode.BadRequest;
                        respons.Success = false;
                        respons.Message = $"Invalid price for product ID {itemDto.ProductId}.";
                        return respons;
                    }
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

                // TotalAmount is calculated automatically - ignore user input 
                
                var finalTotalAmount = new Money(calculatedTotal, currency??"EGP");

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
                        await _unitOfWork.Stocks.UpdateAsync(stock);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                var createdInvoice = await _unitOfWork.Invoice.GetByIdWithItemsAsync(invoice.Id);
                respons.StatusCode=(int)HttpStatusCode.OK;
                respons.Success= true;
                respons.Message = "Success";
                respons.Data = _mapper.Map<InvoiceResponseDto>(createdInvoice);
                return respons;
            }
            catch (BusinessException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ;
            }
        }
        public async Task<GenericResponse<InvoiceResponseDto>> GetInvoiceByIdAsync(int id)
        {
            try
            {
                var respons=new GenericResponse<InvoiceResponseDto>();
                var invoice = await _unitOfWork.Invoice.GetByIdWithItemsAsync(id);
                if (invoice == null)
                {
                    respons.StatusCode=(int)HttpStatusCode.NotFound;
                    respons.Message = $"Invoice with ID {id} not found.";
                    respons.Success= false;
                    return respons;
                   // throw new KeyNotFoundException($"Invoice with ID {id} not found.");
                }
                respons.StatusCode = (int)HttpStatusCode.OK;
                respons.Message = "Success";
                respons.Success = true;
                respons.Data = _mapper.Map<InvoiceResponseDto>(invoice);
                return respons;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<GenericResponse<IEnumerable<InvoiceResponseDto>>> GetAllInvoicesAsync()
        {
            try
            {
                var respons=new GenericResponse<IEnumerable<InvoiceResponseDto>>();
                var invoices = await _unitOfWork.Invoice.GetAllWithItemsAsync();
                respons.StatusCode = (int)HttpStatusCode.OK;
                respons.Message = "Success";
                respons.Success = true;
                respons.Data = _mapper.Map<IEnumerable<InvoiceResponseDto>>(invoices);
                return respons;
               
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<GenericResponse<InvoiceResponseDto>> UpdateInvoiceAsync(int id, InvoiceCreateDto invoiceDto)
        {
            try
            {
                var respons=new GenericResponse<InvoiceResponseDto>();
                if (invoiceDto == null)
                {
                    respons.StatusCode = (int)HttpStatusCode.BadRequest;
                    respons.Success = false;
                    respons.Message = "Invoice data is required.";
                    return respons;
                }

                var existingInvoice = await _unitOfWork.Invoice.GetByIdWithItemsAsync(id);
                if (existingInvoice == null)
                {
                    respons.StatusCode = (int)HttpStatusCode.NotFound;
                    respons.Success = false;
                    respons.Message = $"Invoice with ID {id} not found.";
                    return respons;
                }

                if (!(existingInvoice.Status == Domain.Enums.InvoiceStatus.Draft))
                {
                    respons.StatusCode = (int)HttpStatusCode.BadRequest;
                    respons.Success = false;
                    respons.Message = "Cannot modify an invoice that is Issued or Paid or Cancelled.";
                    return respons;
                }
                var customer = await _unitOfWork.Customer.GetByIdAsync(invoiceDto.CustomerId);
                if (customer == null)
                {
                    respons.StatusCode = (int)HttpStatusCode.NotFound;
                    respons.Success = false;
                    respons.Message = $"Customer with ID {invoiceDto.CustomerId} not found.";
                    return respons;
                }

                var representative = await _unitOfWork.Representatives.GetByIdAsync(invoiceDto.RepresentativeId);
                if (representative == null)
                {
                    respons.StatusCode = (int)HttpStatusCode.NotFound;
                    respons.Success = false;
                    respons.Message = $"Representative with ID {invoiceDto.RepresentativeId} not found.";
                    return respons;
                }

             
                // Calculate new total
                decimal calculatedTotal = 0;
                string Currency = "EGP";
                foreach (var item in invoiceDto.Items)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        respons.StatusCode = (int)HttpStatusCode.NotFound;
                        respons.Success = false;
                        respons.Message = $"Product with ID {item.ProductId} not found.";
                        return respons;
                    }

                    var warehouseId = representative.WarehouseId > 0 ? representative.WarehouseId : 1;
                    var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(item.ProductId, warehouseId);
                    if (stock == null || stock.Quantity < item.Quantity)
                    {
                        respons.StatusCode = (int)HttpStatusCode.NotFound;
                        respons.Success = false;
                        respons.Message = 
                            $"Insufficient stock for product ID {item.ProductId}. Available: {stock?.Quantity ?? 0}, Requested: {item.Quantity}";
                        return respons;
                    }
                    calculatedTotal += item.Quantity * item.Price.Amount;
                    Currency = item.Price.Currency??"EGP";

                }

                // Update basic invoice properties
                existingInvoice.CustomerId = invoiceDto.CustomerId;
                existingInvoice.RepresentativeId = invoiceDto.RepresentativeId;
                existingInvoice.TotalAmount = new Money(calculatedTotal, Currency );
                existingInvoice.UpdatedAt = DateTime.UtcNow;


                // استرجاع الكميات القديمة للمخزون
                var existingItems = (await _unitOfWork.InvoiceItem.GetAllAsync())
                    .Where(ii => ii.InvoiceId == id)
                    .ToList();

                foreach (var oldItem in existingItems)
                {
                    var oldStock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(oldItem.ProductId, representative.WarehouseId);
                    if (oldStock != null)
                    {
                        oldStock.Quantity += oldItem.Quantity;
                        oldStock.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.Stocks.UpdateAsync(oldStock);
                    }
                }
                // Delete existing InvoiceItems
                
                foreach (var item in existingItems)
                {
                    _unitOfWork.InvoiceItem.Remove(item);
                }

                // Add new InvoiceItems
                foreach (var itemDto in invoiceDto.Items)
                {
                    var allowedCurrencies = new[] { "EGP", "$", "USD", "EUR" };
                    itemDto.Price.Currency = itemDto.Price.Currency?.Trim();
                    if (string.IsNullOrWhiteSpace(itemDto.Price.Currency) || !allowedCurrencies.Contains(itemDto.Price.Currency))
                    {
                        respons.StatusCode = (int)HttpStatusCode.BadRequest;
                        respons.Success = false;
                        respons.Message = "Invalid currency";
                        respons.Data = null;
                        return respons;
                    }
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
                        await _unitOfWork.Stocks.UpdateAsync(stock); 
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // Return updated invoice
                var updatedInvoice = await _unitOfWork.Invoice.GetByIdWithItemsAsync(id);
                respons.StatusCode = (int)HttpStatusCode.OK;
                respons.Success = true;
                respons.Message = "Success";
                respons.Data = _mapper.Map<InvoiceResponseDto>(updatedInvoice);
                return respons;
         

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<GenericResponse<InvoiceResponseDto>> DeleteInvoiceAsync(int id)
        {
            try
            {
                var respons = new GenericResponse<InvoiceResponseDto>();
                var invoice = await _unitOfWork.Invoice.GetByIdAsync(id);
                if (invoice == null)
                {
                    respons.StatusCode = (int)HttpStatusCode.NotFound;
                    respons.Success = false;
                    respons.Message = $"Invoice with ID {id} not found.";
                    return respons;
                }

                if (!(invoice.Status == Domain.Enums.InvoiceStatus.Draft))
                {
                    respons.StatusCode = (int)HttpStatusCode.BadRequest;
                    respons.Success = false;
                    respons.Message = "Cannot delete an invoice that is Issued or Paid or Cancelled";
                    return respons;
                }

                // Delete InvoiceItems 
                /*  var items = (await _unitOfWork.InvoiceItem.GetAllAsync())
                      .Where(ii => ii.InvoiceId == id)
                      .ToList();*/
                var items = await _unitOfWork.InvoiceItem.FindAllAsync(ii => ii.InvoiceId == id);
                foreach (var item in items)
                {
                    _unitOfWork.InvoiceItem.Remove(item); 
                }

                await _unitOfWork.Invoice.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                respons.StatusCode = (int)HttpStatusCode.OK;
                respons.Success = true;
                respons.Message = "Success";
                return respons;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
