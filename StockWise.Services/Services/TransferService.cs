using AutoMapper;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.TransferDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class TransferService : ITransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TransferService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TransferResponseDto> CreateTransferAsync(TransferCreateDto transferDto)
        {
            if (transferDto == null)
                throw new ArgumentNullException(nameof(transferDto));

            if (transferDto.Quantity <= 0)
                throw new BusinessException("Quantity must be greater than zero.");

            if (transferDto.FromWarehouseId == transferDto.ToWarehouseId)
                throw new BusinessException("Cannot transfer to the same warehouse.");

            var fromWarehouse = await _unitOfWork.Warehouses.GetWarehouseByIdWithStockAsync(transferDto.FromWarehouseId);
            if (fromWarehouse == null)
                throw new BusinessException($"FromWarehouse with ID {transferDto.FromWarehouseId} not found.");

            var toWarehouse = await _unitOfWork.Warehouses.GetWarehouseByIdWithStockAsync(transferDto.ToWarehouseId);
            if (toWarehouse == null)
                throw new BusinessException($"ToWarehouse with ID {transferDto.ToWarehouseId} not found.");

            var product = await _unitOfWork.Products.GetByIdAsync(transferDto.ProductId);
            if (product == null)
                throw new BusinessException($"Product with ID {transferDto.ProductId} not found.");

            // Check stock availability in FromWarehouse
            var fromStock = fromWarehouse.Stocks.FirstOrDefault(s => s.ProductId == transferDto.ProductId);
            if (fromStock == null || fromStock.Quantity < transferDto.Quantity)
                throw new BusinessException($"Insufficient stock in FromWarehouse ID {transferDto.FromWarehouseId} for Product ID {transferDto.ProductId}. Available: {fromStock?.Quantity ?? 0}, Requested: {transferDto.Quantity}");

            // Update stock in FromWarehouse and ToWarehouse
            fromStock.Quantity -= transferDto.Quantity;
            var toStock = toWarehouse.Stocks.FirstOrDefault(s => s.ProductId == transferDto.ProductId);
            if (toStock != null)
                toStock.Quantity += transferDto.Quantity;
            else
                toWarehouse.Stocks.Add(new Stock { ProductId = transferDto.ProductId, Quantity = transferDto.Quantity });

            var transfer = _mapper.Map<Transfer>(transferDto);
            transfer.CreatedAt = DateTime.UtcNow;
            transfer.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Warehouses.UpdateAsync(fromWarehouse);
            await _unitOfWork.Warehouses.UpdateAsync(toWarehouse);
            await _unitOfWork.Transfers.AddAsync(transfer);
            await _unitOfWork.SaveChangesAsync();

            var createdTransfer = await _unitOfWork.Transfers.GetByIdAsync(transfer.Id);
            return _mapper.Map<TransferResponseDto>(createdTransfer);
        }
        public async Task<IEnumerable<TransferResponseDto>> GetAllTransfersAsync()
        {
            var transfers = await _unitOfWork.Transfers.GetAllAsync();
            return _mapper.Map<IEnumerable<TransferResponseDto>>(transfers);
        }

        public async Task<TransferResponseDto> GetTransferByIdAsync(int id)
        {
            var transfer = await _unitOfWork.Transfers.GetByIdAsync(id);
            if (transfer == null)
                throw new BusinessException($"Transfer with ID {id} not found.");

            return _mapper.Map<TransferResponseDto>(transfer);
        }

        public async Task<TransferResponseDto> UpdateTransferAsync(int id, TransferCreateDto transferDto)
        {
            if (transferDto == null)
                throw new ArgumentNullException(nameof(transferDto));

            if (transferDto.Quantity <= 0)
                throw new BusinessException("Quantity must be greater than zero.");

            if (transferDto.FromWarehouseId == transferDto.ToWarehouseId)
                throw new BusinessException("Cannot transfer to the same warehouse.");

            var existingTransfer = await _unitOfWork.Transfers.GetByIdAsync(id);
            if (existingTransfer == null)
                throw new BusinessException($"Transfer with ID {id} not found.");

            var fromWarehouse = await _unitOfWork.Warehouses.GetWarehouseByIdWithStockAsync(transferDto.FromWarehouseId);
            if (fromWarehouse == null)
                throw new BusinessException($"FromWarehouse with ID {transferDto.FromWarehouseId} not found.");

            var toWarehouse = await _unitOfWork.Warehouses.GetWarehouseByIdWithStockAsync(transferDto.ToWarehouseId);
            if (toWarehouse == null)
                throw new BusinessException($"ToWarehouse with ID {transferDto.ToWarehouseId} not found.");

            var product = await _unitOfWork.Products.GetByIdAsync(transferDto.ProductId);
            if (product == null)
                throw new BusinessException($"Product with ID {transferDto.ProductId} not found.");

            // Revert previous stock changes
            var oldFromWarehouse = await _unitOfWork.Warehouses.GetWarehouseByIdWithStockAsync(existingTransfer.FromWarehouseId);
            var oldToWarehouse = await _unitOfWork.Warehouses.GetWarehouseByIdWithStockAsync(existingTransfer.ToWarehouseId);
            if (oldFromWarehouse != null)
            {
                var oldFromStock = oldFromWarehouse.Stocks.FirstOrDefault(s => s.ProductId == existingTransfer.ProductId);
                if (oldFromStock != null)
                    oldFromStock.Quantity += existingTransfer.Quantity;
            }
            if (oldToWarehouse != null)
            {
                var oldToStock = oldToWarehouse.Stocks.FirstOrDefault(s => s.ProductId == existingTransfer.ProductId);
                if (oldToStock != null)
                    oldToStock.Quantity -= existingTransfer.Quantity;
            }

            await _unitOfWork.Warehouses.UpdateAsync(oldFromWarehouse);
            await _unitOfWork.Warehouses.UpdateAsync(oldToWarehouse);

            // Check new stock availability
            var fromStock = fromWarehouse.Stocks.FirstOrDefault(s => s.ProductId == transferDto.ProductId);
            if (fromStock == null || fromStock.Quantity < transferDto.Quantity)
                throw new BusinessException($"Insufficient stock in FromWarehouse ID {transferDto.FromWarehouseId} for Product ID {transferDto.ProductId}. Available: {fromStock?.Quantity ?? 0}, Requested: {transferDto.Quantity}");

            // Update stock in FromWarehouse and ToWarehouse
            fromStock.Quantity -= transferDto.Quantity;
            var toStock = toWarehouse.Stocks.FirstOrDefault(s => s.ProductId == transferDto.ProductId);
            if (toStock != null)
                toStock.Quantity += transferDto.Quantity;
            else
                toWarehouse.Stocks.Add(new Stock { ProductId = transferDto.ProductId, Quantity = transferDto.Quantity });

            // Update transfer entity
            existingTransfer.FromWarehouseId = transferDto.FromWarehouseId;
            existingTransfer.ToWarehouseId = transferDto.ToWarehouseId;
            existingTransfer.ProductId = transferDto.ProductId;
            existingTransfer.Quantity = transferDto.Quantity;
            existingTransfer.UpdatedAt = DateTime.UtcNow;


            await _unitOfWork.Warehouses.UpdateAsync(fromWarehouse);
            await _unitOfWork.Warehouses.UpdateAsync(toWarehouse);
            await _unitOfWork.Transfers.UpdateAsync(existingTransfer);
            await _unitOfWork.SaveChangesAsync();

            var updatedTransfer = await _unitOfWork.Transfers.GetByIdAsync(id);
            return _mapper.Map<TransferResponseDto>(updatedTransfer);
        }

        public async Task cancelTransferAsync(int id)
        {
            var transfer = await _unitOfWork.Transfers.GetByIdAsync(id);
            if (transfer == null)
                throw new BusinessException($"Transfer with ID {id} not found.");

            // Revert stock changes
            var fromWarehouse = await _unitOfWork.Warehouses.GetWarehouseByIdWithStockAsync(transfer.FromWarehouseId);
            var toWarehouse = await _unitOfWork.Warehouses.GetWarehouseByIdWithStockAsync(transfer.ToWarehouseId);
            if (fromWarehouse != null)
            {
                var fromStock = fromWarehouse.Stocks.FirstOrDefault(s => s.ProductId == transfer.ProductId);
                if (fromStock != null)
                    fromStock.Quantity += transfer.Quantity;
            }
            if (toWarehouse != null)
            {
                var toStock = toWarehouse.Stocks.FirstOrDefault(s => s.ProductId == transfer.ProductId);
                if (toStock != null)
                    toStock.Quantity -= transfer.Quantity;
            }

            await _unitOfWork.Warehouses.UpdateAsync(fromWarehouse);
            await _unitOfWork.Warehouses.UpdateAsync(toWarehouse);
            await _unitOfWork.Transfers.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<TransferResponseDto>> GetByWarehouseIdAsync(int warehouseId)
        {
            var transfers = await _unitOfWork.Transfers.GetAllAsync();
            var filteredTransfers = transfers.Where(t => t.FromWarehouseId == warehouseId || t.ToWarehouseId == warehouseId);
            return _mapper.Map<IEnumerable<TransferResponseDto>>(filteredTransfers);
        }
    }
    }
