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
using StockWise.Services.ServicesResponse;
using System.Net;
using Azure;

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

        public async Task<GenericResponse<InvoiceItemResponseDto>> CreateInvoiceItemAsync(InvoiceItemCreateDto createDto)
        {
            var respons=new GenericResponse<InvoiceItemResponseDto>();
            if (createDto == null)
            {
                respons.StatusCode=(int)HttpStatusCode.BadRequest;
                respons.Success=false;
                respons.Message = "InvoiceItem data is required.";
                return respons;
            }

            if (!createDto.InvoiceId.HasValue || createDto.InvoiceId <= 0)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Valid Invoice Id is required.";
                return respons;
            }

            if (createDto.Quantity <= 0)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Quantity must be greater than zero.";
                return respons;
            }

            if (createDto.Price.Amount < 0)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Price cannot be negative.";
                return respons;
            }
            var allowedCurrencies = new[] { "EGP", "$", "USD", "EUR" };
            createDto.Price.Currency = createDto.Price.Currency?.Trim();
            if (string.IsNullOrWhiteSpace(createDto.Price.Currency) || !allowedCurrencies.Contains(createDto.Price.Currency))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Invalid currency , Allowed values are: EGP , $ , USD , EUR ";
                respons.Data = null;
                return respons;
            }
            var invoice = await _unitOfWork.Invoice.GetByIdAsync(createDto.InvoiceId.Value);
            if (invoice == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = "Invoice not found.";
                return respons;
            }

            var representative = await _unitOfWork.Representatives.GetByIdAsync(invoice.RepresentativeId);
            if (representative == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = "Representative not found.";
                return respons;
            }
            var product = await _unitOfWork.Products.GetByIdAsync(createDto.ProductId);
            if (product == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = "Product not found.";
                return respons;
            }

            //  Stock validation
            var warehouseId = representative.WarehouseId ;
            var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(createDto.ProductId, warehouseId);
            if (stock == null || stock.Quantity < createDto.Quantity)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = $"Insufficient stock for product ID {createDto.ProductId}. Available: {stock?.Quantity ?? 0}, Requested: {createDto.Quantity}";
                return respons;
            }
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
            respons.StatusCode = (int)HttpStatusCode.Created;
            respons.Success = true;
            respons.Message = "Success";
            respons.Data = _mapper.Map<InvoiceItemResponseDto>(createdItem);
            return respons;
           
        }

        public async Task<GenericResponse<InvoiceItemResponseDto>> DeleteInvoiceItemAsync(int id)
        {
            var respons = new GenericResponse<InvoiceItemResponseDto>();
            var invoiceItem = await _unitOfWork.InvoiceItem.GetByIdAsync(id);
            if (invoiceItem == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Invoice item with ID {id} not found.";
                return respons;
            }
            // Draft, Issued, Paid, Cancelled
            var invoice = await _unitOfWork.Invoice.GetByIdAsync(invoiceItem.InvoiceId);

            if (invoice==null ||!invoice.Status.Equals("Draft"))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Cannot delete item from a non-draft invoice";
                return respons;
            }
            //  Restore Stock
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

            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "Invoice item deleted successfully.";
            return respons;
        }

        public async Task<GenericResponse<IEnumerable<InvoiceItemResponseDto>>> GetAllInvoiceItemAsync()
        {
            var respons=new GenericResponse<IEnumerable<InvoiceItemResponseDto>>();
            var invoiceItems = await _unitOfWork.InvoiceItem.GetAllAsync();
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "Success";
            respons.Data = _mapper.Map<IEnumerable<InvoiceItemResponseDto>>(invoiceItems);
            return respons;
        }

        public async Task<GenericResponse<InvoiceItemResponseDto>> GetInvoiceItemByIdAsync(int id)
        {
            var respons=new GenericResponse<InvoiceItemResponseDto> ();
            var invoiceItem = await _unitOfWork.InvoiceItem.GetByIdAsync(id);
            if (invoiceItem == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Invoice item with ID {id} not found.";
                return respons;
            }
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "Success";
            respons.Data = _mapper.Map<InvoiceItemResponseDto>(invoiceItem);
            return respons;
        }

        public async Task<GenericResponse<InvoiceItemResponseDto>> UpdateInvoiceItemAsync(int id, InvoiceItemCreateDto updateDto)
        {
            var respons = new GenericResponse<InvoiceItemResponseDto>();
            if (updateDto == null)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "InvoiceItem data is required.";
                return respons;
            }

            if (!updateDto.InvoiceId.HasValue || updateDto.InvoiceId <= 0)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Valid InvoiceId is required.";
                return respons;
                //throw new BusinessException("Valid InvoiceId is required.");
            }
            var invoice = await _unitOfWork.Invoice.GetByIdAsync(updateDto.InvoiceId.Value);

            if (invoice == null || !invoice.Status.Equals("Draft"))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Cannot delete item from a non-draft invoice";
                return respons;
            }
            var existingInvoiceItem = await _unitOfWork.InvoiceItem.GetByIdAsync(id);
            if (existingInvoiceItem == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Invoice item with ID {id} not found.";
                return respons;
            }

            if (invoice == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = "Invoice not found.";
                return respons;
            }

            var representative = await _unitOfWork.Representatives.GetByIdAsync(invoice.RepresentativeId);
            if (representative == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = "Representative not found.";
                return respons;
            }

            var product = await _unitOfWork.Products.GetByIdAsync(updateDto.ProductId);
            if (product == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = "Product not found.";
                return respons;
            }
            //  Stock validation (مع الـ old quantity)
            var warehouseId = representative.WarehouseId ;
            var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(updateDto.ProductId, warehouseId);
            if (stock == null || stock.Quantity + existingInvoiceItem.Quantity < updateDto.Quantity)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = $"Insufficient stock for product ID {updateDto.ProductId}. Available: {stock?.Quantity + existingInvoiceItem.Quantity ?? 0}, Requested: {updateDto.Quantity}";
                return respons;
            }
            var allowedCurrencies = new[] { "EGP", "$", "USD", "EUR" };
            updateDto.Price.Currency = updateDto.Price.Currency?.Trim();
            if (string.IsNullOrWhiteSpace(updateDto.Price.Currency) || !allowedCurrencies.Contains(updateDto.Price.Currency))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Invalid currency";
                respons.Data = null;
                return respons;
            }
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
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "Success";
            respons.Data = _mapper.Map<InvoiceItemResponseDto>(existingInvoiceItem);
            return respons;
        }
    }
}