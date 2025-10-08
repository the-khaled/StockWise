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
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreatePaymentAsync(PaymentDto paymentDto)
        {
            if (paymentDto == null)
                throw new ArgumentNullException(nameof(paymentDto));

            if (paymentDto.Amount.Amount < 0)
                throw new BusinessException("Payment amount cannot be negative.");

            var invoice = await _unitOfWork.Invoice.GetByIdAsync(paymentDto.InvoiceId);
            if (invoice == null)
                throw new BusinessException("Invoice not found.");

            var customer = await _unitOfWork.Customer.GetByIdAsync(paymentDto.CustomerId);
            if (customer == null)
                throw new BusinessException("Customer not found.");

            await _unitOfWork.Payment.AddAsync(MapToEntity(paymentDto));
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeletePaymentAsync(int id)
        {
            var payment = await _unitOfWork.Payment.GetByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found.");

            await _unitOfWork.Payment.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentAsync()
        {
            var payment =await _unitOfWork.Payment.GetAllAsync();
            return payment.Select(s=>MapToDto(s)).ToList();
        }

        public async Task<PaymentDto> GetPaymentByIdAsync(int id)
        {
            var payment = await _unitOfWork.Payment.GetByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found.");

            return MapToDto(payment);
        }

        public async Task UpdatePaymentAsync(PaymentDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var existingPayment = await _unitOfWork.Payment.GetByIdAsync(dto.Id);
            if (existingPayment == null)
                throw new KeyNotFoundException($"Payment with ID {dto.Id} not found.");


            var invoice = await _unitOfWork.Invoice.GetByIdAsync(dto.InvoiceId);
            if (invoice == null)
                throw new BusinessException("Invoice not found.");

            var customer = await _unitOfWork.Customer.GetByIdAsync(dto.CustomerId);
            if (customer == null)
                throw new BusinessException("Customer not found.");

            existingPayment.InvoiceId = dto.InvoiceId;
            existingPayment.CustomerId = dto.CustomerId;
            existingPayment.Amount = dto.Amount;
            existingPayment.Method = dto.Method;
            existingPayment.Status = dto.Status;
            existingPayment.UpdatedAt = DateTime.Now;

            await _unitOfWork.Payment.UpdateAsync(existingPayment);
            await _unitOfWork.SaveChangesAsync();
        }
        public PaymentDto MapToDto(Payment payment) 
        {
            return new PaymentDto 
            {
                Id = payment.Id,
                InvoiceId = payment.InvoiceId,
                CustomerId = payment.CustomerId,
                Amount = payment.Amount,
                Method = payment.Method,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }
        public Payment MapToEntity(PaymentDto paymentdto)
        {
            return new Payment
            {
                Id = paymentdto.Id,
                InvoiceId = paymentdto.InvoiceId,
                CustomerId = paymentdto.CustomerId,
                Amount = paymentdto.Amount,
                Method = paymentdto.Method,
                Status = paymentdto.Status,
                CreatedAt = paymentdto.CreatedAt,
                UpdatedAt = paymentdto.UpdatedAt
            };
        }
    }
}
