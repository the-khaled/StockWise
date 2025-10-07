using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
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
        private IUnitOfWork _unitOfWork;
        public TransferService( IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreateTransferAsync(TransferDto transferdto)
        {
            if (transferdto == null) throw new ArgumentNullException(nameof(transferdto));
             if (transferdto.Quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
            //التاكد ان المخذنين موجودين 
            var fromwarehouse = await _unitOfWork.Warehouses.GetByIdAsync(transferdto.FromWarehouseId);
            var towarehouse = await _unitOfWork.Warehouses.GetByIdAsync(transferdto.ToWarehouseId);
            //التاكد ان الكميه مش سالبه 
           if (fromwarehouse == null || towarehouse == null) throw new BusinessException("One or both warehouses not found.");
            //التاكد ان المنتج نفسو موجود
            var prod = await _unitOfWork.Products.GetByIdAsync(transferdto.ProductId);
            if (prod == null) throw new BusinessException("Product Not Found");
            var transfer = MapToEntity(transferdto);
            await _unitOfWork.Transfers.AddAsync(transfer);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<IEnumerable<TransferDto>> GetAllTransfersAsync()
        {
            var transfers = await _unitOfWork.Transfers.GetAllAsync();
            return transfers.Select(t => MapToDto(t)).ToList();
        }

        public async Task<TransferDto> GetTransferByIdAsync(int id)
        {
            var transfer=await _unitOfWork.Transfers.GetByIdAsync(id);
            if (transfer == null) { throw new KeyNotFoundException($"Transfer with ID {id} not found."); }
            return MapToDto(transfer);
        }

        public async Task UpdateTransferAsync(TransferDto transferdto)
        {
            if (transferdto == null) throw new ArgumentNullException(nameof(transferdto));
            var transferexist = await _unitOfWork.Transfers.GetByIdAsync(transferdto.Id);
            if (transferexist == null)
                throw new KeyNotFoundException($"Transfer with ID {transferdto.Id} not found.");
            transferexist.FromWarehouseId=transferdto.FromWarehouseId;
            transferexist.ToWarehouseId=transferdto.ToWarehouseId;
            transferexist.ProductId=transferdto.ProductId;
            transferexist.Quantity=transferdto.Quantity;
            transferexist.UpdatedAt=DateTime.Now;
            await _unitOfWork.Transfers.UpdateAsync(transferexist);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task DeleteTransferAsync(int id)
        {
            var transfere = await _unitOfWork.Transfers.GetByIdAsync(id);
            if (transfere == null) throw new KeyNotFoundException($"Transfer with ID {id} not found.");
            await _unitOfWork.Transfers.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
        public TransferDto MapToDto(Transfer transfer) 
        {
            return new TransferDto
            {
                Id = transfer.Id,
                FromWarehouseId = transfer.FromWarehouseId,
                ToWarehouseId = transfer.ToWarehouseId,
                ProductId = transfer.ProductId,
                Quantity = transfer.Quantity,
                CreatedAt = transfer.CreatedAt,
                UpdatedAt = transfer.UpdatedAt
            };
        }
        private Transfer MapToEntity(TransferDto Dto)
        {
            return new Transfer
            {
                Id = Dto.Id,
                FromWarehouseId = Dto.FromWarehouseId,
                ToWarehouseId = Dto.ToWarehouseId,
                ProductId = Dto.ProductId,
                Quantity = Dto.Quantity,
                CreatedAt = Dto.CreatedAt,
                UpdatedAt = Dto.UpdatedAt
            };
        }
    }
}
