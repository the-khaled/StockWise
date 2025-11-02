using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Domain.ValueObjects;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.ReturnDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class ReturnService : IReturnService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ReturnService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ReturnResponseDto> CreateReturnAsync(ReturnCreateDto returnDto)
        {
            if (returnDto == null)
                throw new ArgumentNullException(nameof(returnDto));

            if (returnDto.Quantity <= 0)
                throw new BusinessException("Quantity must be greater than zero.");

            if (returnDto.RepresentativeId.HasValue == returnDto.CustomerId.HasValue)
                throw new BusinessException("Exactly one of RepresentativeId or CustomerId must be provided.");

            var product = await _unitOfWork.Products.GetByIdAsync(returnDto.ProductId); // Changed from Products
            if (product == null)
                throw new BusinessException("Product not found.");

            Representative representative = null;
            int? warehouseId = null;
            Invoice invoice = null;
            if (returnDto.RepresentativeId.HasValue)
            {
                representative = await _unitOfWork.Representatives.GetByIdAsync(returnDto.RepresentativeId.Value); // Changed from Representatives
                if (representative == null)
                    throw new BusinessException($"Representative with ID {returnDto.RepresentativeId.Value} not found.");

                warehouseId = representative.WarehouseId;

                // Validate if the representative sold this product with sufficient quantity
                invoice = await _unitOfWork.Invoice.GetQueryable()
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.RepresentativeId == returnDto.RepresentativeId.Value
                        && i.Items.Any(it => it.ProductId == returnDto.ProductId && it.Quantity >= returnDto.Quantity));
                if (invoice == null)
                    throw new BusinessException($"No valid invoice found for Representative ID {returnDto.RepresentativeId.Value} with Product ID {returnDto.ProductId} and sufficient quantity (Requested: {returnDto.Quantity}).");
            }
            else if (returnDto.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customer.GetByIdAsync(returnDto.CustomerId.Value);
                if (customer == null)
                    throw new BusinessException($"Customer with ID {returnDto.CustomerId.Value} not found.");

                // Validate if the customer bought this product with sufficient quantity
                invoice = await _unitOfWork.Invoice.GetQueryable()
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.CustomerId == returnDto.CustomerId.Value
                        && i.Items.Any(it => it.ProductId == returnDto.ProductId && it.Quantity >= returnDto.Quantity));
                if (invoice == null)
                    throw new BusinessException($"No valid invoice found for Customer ID {returnDto.CustomerId.Value} with Product ID {returnDto.ProductId} and sufficient quantity (Requested: {returnDto.Quantity}).");
            }

            var returnEntity = _mapper.Map<Return>(returnDto);
            returnEntity.CreatedAt = DateTime.UtcNow;
            returnEntity.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Return.AddAsync(returnEntity);
            await _unitOfWork.SaveChangesAsync();

            // Business Logic: If condition is Damaged, add to Expense and DamagedProducts
            if (returnDto.Condition == Domain.Enums.ProductCondition.Damaged)
            {
                if (product.Price == null || product.Price.Amount <= 0)
                    throw new BusinessException($"Invalid Price for Product ID {returnDto.ProductId}.");

                // Add to Expense
                var expense = new Expense
                {
                    Description = $"Damaged product return for Product ID {returnDto.ProductId}",
                    Amount = new Money(product.Price.Amount, product.Price.Currency),
                    ExpenseType = Domain.Enums.ExpenseType.General,
                    RepresentativeId = returnDto.RepresentativeId
                };
                await _unitOfWork.Expense.AddAsync(expense);

                // Add to DamagedProducts
                var damagedProduct = new DamagedProduct
                {
                    ProductId = returnDto.ProductId,
                    Quantity = returnDto.Quantity,
                    Reason = returnDto.Reason,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.DamagedProduct.AddAsync(damagedProduct);

                await _unitOfWork.SaveChangesAsync();
            }
            else if (returnDto.Condition == Domain.Enums.ProductCondition.Good)
            {
                // Add back to Stock if condition is Good
                warehouseId = returnDto.RepresentativeId.HasValue ? warehouseId : 1; // Assume 1 is Main Warehouse ID for FromCustomer returns
                if (!warehouseId.HasValue)
                    throw new BusinessException("Warehouse ID is not specified.");
                var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(returnDto.ProductId, warehouseId.Value); // Changed from Stocks
                if (stock == null)
                {
                    stock = new Stock
                    {
                        ProductId = returnDto.ProductId,
                        WarehouseId = warehouseId.Value,
                        Quantity = returnDto.Quantity,
                        MinQuantity = 0
                    };
                    await _unitOfWork.Stocks.AddAsync(stock); // Changed from Stocks
                }
                else
                {
                    stock.Quantity += returnDto.Quantity;
                    await _unitOfWork.Stocks.UpdateAsync(stock); // Changed from Stocks
                }
                await _unitOfWork.SaveChangesAsync();
            }

            // Update Invoice: Adjust quantity in InvoiceItem
            if (invoice != null)
            {
                var invoiceItem = invoice.Items.FirstOrDefault(it => it.ProductId == returnDto.ProductId);
                if (invoiceItem == null)
                    throw new BusinessException($"Product ID {returnDto.ProductId} not found in invoice.");

                invoiceItem.Quantity -= returnDto.Quantity;
                if (invoiceItem.Quantity < 0)
                    throw new BusinessException($"Invalid quantity adjustment for Product ID {returnDto.ProductId} in invoice.");

                // Update Invoice TotalAmount if exists
                if (invoice.TotalAmount != null)
                {
                    if (product.Price == null || product.Price.Amount <= 0)
                        throw new BusinessException($"Invalid Price for Product ID {returnDto.ProductId}.");
                    invoice.TotalAmount.Amount -= product.Price.Amount * returnDto.Quantity;
                    if (invoice.TotalAmount.Amount < 0)
                        invoice.TotalAmount.Amount = 0;
                }

                // If all items have quantity 0, cancel the invoice
                if (invoice.Items.All(it => it.Quantity == 0))
                {
                    invoice.Status = Domain.Enums.InvoiceStatus.Cancelled;
                }
                else if (invoice.Status == Domain.Enums.InvoiceStatus.Paid)
                {
                    // If paid, don't change status, just adjust quantities
                }
                else
                {
                    invoice.Status = Domain.Enums.InvoiceStatus.Issued; // Ensure it stays Issued if partially returned
                }

                await _unitOfWork.Invoice.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // If invoice not paid and associated with a customer, adjust credit balance
                if (invoice.Status != Domain.Enums.InvoiceStatus.Paid && invoice.CustomerId != 0)
                {
                    var customerCredit = await _unitOfWork.Customer.GetByIdAsync(invoice.CustomerId);
                    if (customerCredit != null)
                    {
                        if (product.Price == null || product.Price.Amount <= 0)
                            throw new BusinessException($"Invalid Price for Product ID {returnDto.ProductId}.");
                        if (customerCredit.CreditBalance == null)
                            throw new BusinessException($"CreditBalance is not initialized for Customer ID {invoice.CustomerId}.");
                        if (product.Price.Currency != customerCredit.CreditBalance.Currency)
                            throw new BusinessException($"Currency mismatch: Product Price ({product.Price.Currency}) does not match Customer CreditBalance ({customerCredit.CreditBalance.Currency}).");
                        decimal returnAmount = product.Price.Amount * returnDto.Quantity;
                        customerCredit.CreditBalance.Amount -= returnAmount;
                        if (customerCredit.CreditBalance.Amount < 0)
                            customerCredit.CreditBalance.Amount = 0;
                        await _unitOfWork.Customer.UpdateAsync(customerCredit);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
            }

            var createdReturn = await _unitOfWork.Return.GetByIdAsync(returnEntity.Id);
            return _mapper.Map<ReturnResponseDto>(createdReturn);
        }
        public async Task DeleteReturnAsync(int id)
        {
            var returnEntity = await _unitOfWork.Return.GetByIdAsync(id);
            if (returnEntity == null)
                throw new KeyNotFoundException($"Return with ID {id} not found.");

            await _unitOfWork.Return.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ReturnResponseDto>> GetAllReturnAsync()
        {
            var returndto = await _unitOfWork.Return.GetAllAsync();
            return _mapper.Map<IEnumerable<ReturnResponseDto>>(returndto);
        }

        public async Task<IEnumerable<ReturnResponseDto>> GetReturnsByCustomerIdAsync(int customerId)
        {
            var Customer = await _unitOfWork.Products.GetByIdAsync(customerId);
            if (Customer == null)
                throw new KeyNotFoundException($"customer with ID {customerId} not found.");
            // validation if customer desnt returned 
            var returns = await _unitOfWork.Return.GetByProductIdAsync(customerId);
            return _mapper.Map<IEnumerable<ReturnResponseDto>>(returns);
        }

        public async Task<ReturnResponseDto> GetReturnsByIdAsync(int id)
        {
            var returndto = await _unitOfWork.Return.GetByIdAsync(id);
            if (returndto == null)
                throw new KeyNotFoundException($"Return with ID {id} not found.");
            return _mapper.Map<ReturnResponseDto>(returndto);
        }

        public async Task<IEnumerable<ReturnResponseDto>> GetReturnsByProductIdAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            // validation if product desnt returned 
            var returns = await _unitOfWork.Return.GetByProductIdAsync(productId);
            return _mapper.Map<IEnumerable<ReturnResponseDto>>(returns);
        }

        public async Task<IEnumerable<ReturnResponseDto>> GetReturnsByRepresentativeIdAsync(int representativeId)
        {
            var Representative = await _unitOfWork.Representatives.GetByIdAsync(representativeId);
            if (Representative == null)
                throw new KeyNotFoundException($"Representative with ID {representativeId} not found.");
            // validation if representative desnt returned 
            var returns = await _unitOfWork.Return.GetByProductIdAsync(representativeId);
            return _mapper.Map<IEnumerable<ReturnResponseDto>>(returns);
        }

        public async Task<ReturnResponseDto> UpdateReturnAsync(int id, ReturnCreateDto returnDto)
        {
            if (returnDto == null)
                throw new ArgumentNullException(nameof(returnDto));

            var existingReturn = await _unitOfWork.Return.GetByIdAsync(id);
            if (existingReturn == null)
                throw new KeyNotFoundException($"Return with ID {id} not found.");

            if (returnDto.RepresentativeId.HasValue == returnDto.CustomerId.HasValue)
                throw new BusinessException("Exactly one of RepresentativeId or CustomerId must be provided.");

            var product = await _unitOfWork.Products.GetByIdAsync(returnDto.ProductId);
            if (product == null)
                throw new BusinessException("Product not found.");

            Representative representative = null;
            int? warehouseId = null;
            Invoice oldInvoice = null;
            if (existingReturn.RepresentativeId.HasValue)
            {
                var oldRepresentative = await _unitOfWork.Representatives.GetByIdAsync(existingReturn.RepresentativeId.Value);
                if (oldRepresentative != null)
                {
                    // Revert previous Stock changes if Good
                    if (existingReturn.Condition == Domain.Enums.ProductCondition.Good)
                    {
                        var oldStock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(existingReturn.ProductId, oldRepresentative.WarehouseId);
                        if (oldStock != null)
                        {
                            oldStock.Quantity -= existingReturn.Quantity; // Revert previous quantity
                            if (oldStock.Quantity < 0)
                                oldStock.Quantity = 0; // Prevent negative stock
                            await _unitOfWork.Stocks.UpdateAsync(oldStock);
                        }
                    }

                    // Revert previous Invoice status
                    oldInvoice = await _unitOfWork.Invoice.GetQueryable()
                        .Include(i => i.Items)
                        .FirstOrDefaultAsync(i => i.RepresentativeId == existingReturn.RepresentativeId.Value
                            && i.Items.Any(it => it.ProductId == existingReturn.ProductId));
                    if (oldInvoice != null)
                    {
                        oldInvoice.Status = Domain.Enums.InvoiceStatus.Issued; // Revert to Issued
                        await _unitOfWork.Invoice.UpdateAsync(oldInvoice);

                        // Revert CreditBalance if not paid
                        if (oldInvoice.Status != Domain.Enums.InvoiceStatus.Paid && oldInvoice.CustomerId != 0)
                        {
                            var customerCredit = await _unitOfWork.Customer.GetByIdAsync(oldInvoice.CustomerId);
                            if (customerCredit != null)
                            {
                                var oldProduct = await _unitOfWork.Products.GetByIdAsync(existingReturn.ProductId);
                                if (oldProduct != null && oldProduct.Price != null && oldProduct.Price.Amount > 0)
                                {
                                    if (customerCredit.CreditBalance == null)
                                        throw new BusinessException($"CreditBalance is not initialized for Customer ID {oldInvoice.CustomerId}.");
                                    if (oldProduct.Price.Currency != customerCredit.CreditBalance.Currency)
                                        throw new BusinessException($"Currency mismatch: Product Price ({oldProduct.Price.Currency}) does not match Customer CreditBalance ({customerCredit.CreditBalance.Currency}).");
                                    customerCredit.CreditBalance.Amount += oldProduct.Price.Amount * existingReturn.Quantity; // Revert credit balance
                                    await _unitOfWork.Customer.UpdateAsync(customerCredit);
                                }
                            }
                        }
                    }
                }
            }
            else if (existingReturn.CustomerId.HasValue)
            {
                // Revert previous Invoice status
                oldInvoice = await _unitOfWork.Invoice.GetQueryable()
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.CustomerId == existingReturn.CustomerId.Value
                        && i.Items.Any(it => it.ProductId == existingReturn.ProductId));
                if (oldInvoice != null)
                {
                    oldInvoice.Status = Domain.Enums.InvoiceStatus.Issued; // Revert to Issued
                    await _unitOfWork.Invoice.UpdateAsync(oldInvoice);

                    // Revert CreditBalance if not paid
                    if (oldInvoice.Status != Domain.Enums.InvoiceStatus.Paid && oldInvoice.CustomerId != 0)
                    {
                        var customerCredit = await _unitOfWork.Customer.GetByIdAsync(oldInvoice.CustomerId);
                        if (customerCredit != null)
                        {
                            var oldProduct = await _unitOfWork.Products.GetByIdAsync(existingReturn.ProductId);
                            if (oldProduct != null && oldProduct.Price != null && oldProduct.Price.Amount > 0)
                            {
                                if (customerCredit.CreditBalance == null)
                                    throw new BusinessException($"CreditBalance is not initialized for Customer ID {oldInvoice.CustomerId}.");
                                if (oldProduct.Price.Currency != customerCredit.CreditBalance.Currency)
                                    throw new BusinessException($"Currency mismatch: Product Price ({oldProduct.Price.Currency}) does not match Customer CreditBalance ({customerCredit.CreditBalance.Currency}).");
                                customerCredit.CreditBalance.Amount += oldProduct.Price.Amount * existingReturn.Quantity; // Revert credit balance
                                await _unitOfWork.Customer.UpdateAsync(customerCredit);
                            }
                        }
                    }
                }
            }

            if (returnDto.RepresentativeId.HasValue)
            {
                representative = await _unitOfWork.Representatives.GetByIdAsync(returnDto.RepresentativeId.Value);
                if (representative == null)
                    throw new BusinessException($"Representative with ID {returnDto.RepresentativeId.Value} not found.");

                warehouseId = representative.WarehouseId;

                // Validate if the representative sold this product with sufficient quantity
                var invoiceFromRepresentative = await _unitOfWork.Invoice.GetQueryable()
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.RepresentativeId == returnDto.RepresentativeId.Value
                        && i.Items.Any(it => it.ProductId == returnDto.ProductId && it.Quantity >= returnDto.Quantity));
                if (invoiceFromRepresentative == null)
                    throw new BusinessException($"No valid invoice found for Representative ID {returnDto.RepresentativeId.Value} with Product ID {returnDto.ProductId} and sufficient quantity (Requested: {returnDto.Quantity}).");
            }
            else if (returnDto.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customer.GetByIdAsync(returnDto.CustomerId.Value);
                if (customer == null)
                    throw new BusinessException($"Customer with ID {returnDto.CustomerId.Value} not found.");

                // Validate if the customer bought this product with sufficient quantity
                var invoiceToCustomer = await _unitOfWork.Invoice.GetQueryable()
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.CustomerId == returnDto.CustomerId.Value
                        && i.Items.Any(it => it.ProductId == returnDto.ProductId && it.Quantity >= returnDto.Quantity));
                if (invoiceToCustomer == null)
                    throw new BusinessException($"No valid invoice found for Customer ID {returnDto.CustomerId.Value} with Product ID {returnDto.ProductId} and sufficient quantity (Requested: {returnDto.Quantity}).");
            }

            _mapper.Map(returnDto, existingReturn);
            existingReturn.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Return.UpdateAsync(existingReturn);
            await _unitOfWork.SaveChangesAsync();

            // Business Logic: If condition is Damaged, add to Expense
            if (returnDto.Condition == Domain.Enums.ProductCondition.Damaged)
            {
                if (product.Price == null || product.Price.Amount <= 0)
                    throw new BusinessException($"Invalid Price for Product ID {returnDto.ProductId}.");
                var expense = new Expense
                {
                    Description = $"Damaged product return for Product ID {returnDto.ProductId}",
                    Amount = new Money(product.Price.Amount,product.Price.Currency), // Use Price.Amount
                    ExpenseType = Domain.Enums.ExpenseType.General,
                    RepresentativeId = returnDto.RepresentativeId
                };
                await _unitOfWork.Expense.AddAsync(expense);
                await _unitOfWork.SaveChangesAsync();
            }
            else if (returnDto.Condition == Domain.Enums.ProductCondition.Good)
            {
                // Add back to Stock if condition is Good
                warehouseId = returnDto.RepresentativeId.HasValue ? warehouseId : 1; // Assume 1 is Main Warehouse ID for FromCustomer returns
                if (!warehouseId.HasValue)
                    throw new BusinessException("Warehouse ID is not specified.");
                var stock = await _unitOfWork.Stocks.GetByWarehouseAndProductAsync(returnDto.ProductId, warehouseId.Value);
                if (stock == null)
                {
                    stock = new Stock
                    {
                        ProductId = returnDto.ProductId,
                        WarehouseId = warehouseId.Value,
                        Quantity = returnDto.Quantity,
                        MinQuantity = 0
                    };
                    await _unitOfWork.Stocks.AddAsync(stock);
                }
                else
                {
                    stock.Quantity += returnDto.Quantity;
                    await _unitOfWork.Stocks.UpdateAsync(stock);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            // Update Invoice status to Cancelled and validate quantity
            Invoice invoice = null;
            if (returnDto.RepresentativeId.HasValue)
            {
                invoice = await _unitOfWork.Invoice.GetQueryable()
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.RepresentativeId == returnDto.RepresentativeId.Value
                        && i.Items.Any(it => it.ProductId == returnDto.ProductId && it.Quantity >= returnDto.Quantity));
                if (invoice == null)
                    throw new BusinessException($"No valid invoice found for Representative ID {returnDto.RepresentativeId.Value} with Product ID {returnDto.ProductId} and sufficient quantity (Requested: {returnDto.Quantity}).");
            }
            else if (returnDto.CustomerId.HasValue)
            {
                invoice = await _unitOfWork.Invoice.GetQueryable()
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.CustomerId == returnDto.CustomerId.Value
                        && i.Items.Any(it => it.ProductId == returnDto.ProductId && it.Quantity >= returnDto.Quantity));
                if (invoice == null)
                    throw new BusinessException($"No valid invoice found for Customer ID {returnDto.CustomerId.Value} with Product ID {returnDto.ProductId} and sufficient quantity (Requested: {returnDto.Quantity}).");
            }

            if (invoice != null)
            {
                invoice.Status = Domain.Enums.InvoiceStatus.Cancelled;
                await _unitOfWork.Invoice.UpdateAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                // If invoice not paid and associated with a customer, adjust credit balance
                if (invoice.Status != Domain.Enums.InvoiceStatus.Paid && invoice.CustomerId != 0)
                {
                    var customerCredit = await _unitOfWork.Customer.GetByIdAsync(invoice.CustomerId);
                    if (customerCredit != null)
                    {
                        if (product.Price == null || product.Price.Amount <= 0)
                            throw new BusinessException($"Invalid Price for Product ID {returnDto.ProductId}.");
                        if (customerCredit.CreditBalance == null)
                            throw new BusinessException($"CreditBalance is not initialized for Customer ID {invoice.CustomerId}.");
                        if (product.Price.Currency != customerCredit.CreditBalance.Currency)
                            throw new BusinessException($"Currency mismatch: Product Price ({product.Price.Currency}) does not match Customer CreditBalance ({customerCredit.CreditBalance.Currency}).");
                        decimal returnAmount = product.Price.Amount * returnDto.Quantity;
                        customerCredit.CreditBalance.Amount -= returnAmount; // Adjust credit balance
                        if (customerCredit.CreditBalance.Amount < 0)
                            customerCredit.CreditBalance.Amount = 0; // Prevent negative balance
                        await _unitOfWork.Customer.UpdateAsync(customerCredit);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
            }

            return _mapper.Map<ReturnResponseDto>(existingReturn);
        }
    }             
      
    }

