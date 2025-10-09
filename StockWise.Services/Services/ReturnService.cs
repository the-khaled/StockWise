using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class ReturnService : IReturnService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReturnService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreateReturnAsync(ReturnDto ReturnDto)
        {
            if (ReturnDto == null)
                throw new ArgumentNullException(nameof(ReturnDto));

            if (ReturnDto.Quantity <= 0)
                throw new BusinessException("Quantity must be greater than zero.");

            var product = await _unitOfWork.Products.GetByIdAsync(ReturnDto.ProductId);
            if (product == null)
                throw new BusinessException("Product not found.");

            if (ReturnDto.RepresentativeId.HasValue)
            {
                var representative = await _unitOfWork.Representatives.GetByIdAsync(ReturnDto.RepresentativeId.Value);
                if (representative == null)
                    throw new BusinessException("Representative not found.");
            }

            if (ReturnDto.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customer.GetByIdAsync(ReturnDto.CustomerId.Value);
                if (customer == null)
                    throw new BusinessException("Customer not found.");
            }

            await _unitOfWork.Return.AddAsync(MapToEntity(ReturnDto));
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteReturnAsync(int id)
        {
            var returnEntity = await _unitOfWork.Return.GetByIdAsync(id);
            if (returnEntity == null)
                throw new KeyNotFoundException($"Return with ID {id} not found.");

            await _unitOfWork.Return.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ReturnDto>> GetAllReturnAsync()
        {
            var returndto= await _unitOfWork.Return.GetAllAsync();
            return returndto.Select(r=>MapToDto(r)).ToList();
        }

        public async Task<ReturnDto> GetReturnsByIdAsync(int id)
        {
            var returndto =await _unitOfWork.Return.GetByIdAsync(id);
            if (returndto == null)
                throw new KeyNotFoundException($"Return with ID {id} not found.");
            return MapToDto(returndto);
        }

        public async Task UpdateReturnAsync(ReturnDto ReturnDto)
        {
            if (ReturnDto == null)
                throw new ArgumentNullException(nameof(ReturnDto));

            var existingReturn = await _unitOfWork.Return.GetByIdAsync(ReturnDto.Id);
            if (existingReturn == null)
                 throw new KeyNotFoundException($"Return with ID {ReturnDto.Id} not found.");
            

            var product = await _unitOfWork.Products.GetByIdAsync(ReturnDto.ProductId);
            if (product == null)
                throw new BusinessException("Product not found.");

            if (ReturnDto.RepresentativeId.HasValue)
            {
                var representative = await _unitOfWork.Representatives.GetByIdAsync(ReturnDto.RepresentativeId.Value);
                if (representative == null)
                    throw new BusinessException("Representative not found.");
            }

            if (ReturnDto.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customer.GetByIdAsync(ReturnDto.CustomerId.Value);
                if (customer == null)
                    throw new BusinessException("Customer not found.");
            }

            existingReturn.ProductId = ReturnDto.ProductId;
            existingReturn.RepresentativeId = ReturnDto.RepresentativeId;
            existingReturn.CustomerId = ReturnDto.CustomerId;
            existingReturn.Quantity = ReturnDto.Quantity;
            existingReturn.ReturnType = ReturnDto.ReturnType;
            existingReturn.Reason = ReturnDto.Reason;   
            existingReturn.UpdatedAt = DateTime.Now;

            await _unitOfWork.Return.UpdateAsync(existingReturn);
            await _unitOfWork.SaveChangesAsync();
        }
        public ReturnDto MapToDto(Return Entityreturn)
        {
            return new ReturnDto
            { 
                Id = Entityreturn.Id,
                ProductId = Entityreturn.ProductId,
                RepresentativeId=Entityreturn.RepresentativeId,
                CustomerId =Entityreturn.CustomerId,
                Quantity = Entityreturn.Quantity,
                Reason = Entityreturn.Reason,
                ReturnType=Entityreturn.ReturnType,
                CreatedAt = Entityreturn.CreatedAt,
                UpdatedAt = Entityreturn.UpdatedAt,
            };
        }
        public Return MapToEntity(ReturnDto dto)
        {
            return new Return
            {
                Id = dto.Id,
                ProductId = dto.ProductId,
                RepresentativeId = dto.RepresentativeId,
                CustomerId = dto.CustomerId,
                Quantity = dto.Quantity,
                Reason= dto.Reason,
                ReturnType = dto.ReturnType,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
            };
        }

     

      
    }
}
/*
  public int Id { get; set; } 
        [Required(ErrorMessage = "ProductId is required.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "RepresentativeId is required.")]
        public int RepresentativeId { get; set; }

        [Required(ErrorMessage = "CustomerId is required.")]
        public int CustomerId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "ReturnType is required.")]
        public ReturnType ReturnType { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
 */