using AutoMapper;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Domain.ValueObjects;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.PaymentDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<PaymentResponseDto> CreatePaymentAsync(PaymentCreateDto paymentDto)
        {
            if (paymentDto == null)
                throw new ArgumentNullException(nameof(paymentDto));

            if (paymentDto.Amount.Amount < 0|| paymentDto.Amount == null)
                throw new BusinessException("Payment amount cannot be negative.");

            var invoice = await _unitOfWork.Invoice.GetByIdAsync(paymentDto.InvoiceId);
            if (invoice == null)
                throw new BusinessException("Invoice not found.");

            var customer = await _unitOfWork.Customer.GetByIdAsync(paymentDto.CustomerId);
            if (customer == null)
                throw new BusinessException("Customer not found.");
            if (string.IsNullOrEmpty(paymentDto.TransactionId))
                paymentDto.TransactionId = Guid.NewGuid().ToString();
            if (invoice.CustomerId != paymentDto.CustomerId)
                throw new BusinessException($"Invoice {paymentDto.InvoiceId} is not associated with Customer {paymentDto.CustomerId}.");

            if (customer.CreditBalance == null)
                throw new BusinessException($"CreditBalance is not initialized for Customer ID {paymentDto.CustomerId}.");

            if (paymentDto.Amount.Currency != customer.CreditBalance.Currency)
                throw new BusinessException($"Currency mismatch: Payment ({paymentDto.Amount.Currency}) does not match Customer CreditBalance ({customer.CreditBalance.Currency}).");
            var paymentEntity = _mapper.Map<Payment>(paymentDto);
            paymentEntity.CreatedAt = DateTime.UtcNow;
            paymentEntity.UpdatedAt = DateTime.UtcNow;
            paymentEntity.TransactionId =
                string.IsNullOrEmpty(paymentDto.TransactionId) ? Guid.NewGuid().ToString() : paymentDto.TransactionId;
            // Update Invoice Status
            if (paymentDto.Status == PaymentStatus.Completed)
            {
                invoice.Status = InvoiceStatus.Paid;
                await _unitOfWork.Invoice.UpdateAsync(invoice);
            }
            // Update Customer CreditBalance
            customer.CreditBalance.Amount -= paymentDto.Amount.Amount;
            if (customer.CreditBalance.Amount < 0)
                customer.CreditBalance.Amount = 0;
            await _unitOfWork.Customer.UpdateAsync(customer);
            await _unitOfWork.Payment.AddAsync(paymentEntity);
            await _unitOfWork.SaveChangesAsync();
            var createdPayment = await _unitOfWork.Payment.GetByIdAsync(paymentEntity.Id);
            return _mapper.Map<PaymentResponseDto>(createdPayment);
        }
        public async Task<PaymentResponseDto> UpdatePaymentAsync(int id,PaymentCreateDto paymentDto)
        {
            if (paymentDto == null)
                throw new ArgumentNullException(nameof(paymentDto));

            if (paymentDto.Amount == null || paymentDto.Amount.Amount <= 0)
                throw new BusinessException("Payment amount must be greater than zero.");

            var existingPayment = await _unitOfWork.Payment.GetByIdAsync(id);
            if (existingPayment == null)
                throw new BusinessException($"Payment with ID {id} not found.");

            var invoice = await _unitOfWork.Invoice.GetByIdAsync(paymentDto.InvoiceId);
            if (invoice == null)
                throw new BusinessException($"Invoice with ID {paymentDto.InvoiceId} not found.");

            var customer = await _unitOfWork.Customer.GetByIdAsync(paymentDto.CustomerId);
            if (customer == null)
                throw new BusinessException($"Customer with ID {paymentDto.CustomerId} not found.");

            if (invoice.CustomerId != paymentDto.CustomerId)
                throw new BusinessException($"Invoice {paymentDto.InvoiceId} is not associated with Customer {paymentDto.CustomerId}.");

            if (customer.CreditBalance == null)
                throw new BusinessException($"CreditBalance is not initialized for Customer ID {paymentDto.CustomerId}.");

            if (paymentDto.Amount.Currency != customer.CreditBalance.Currency)
                throw new BusinessException($"Currency mismatch: Payment ({paymentDto.Amount.Currency}) does not match Customer CreditBalance ({customer.CreditBalance.Currency}).");

            // Revert previous payment effect on CreditBalance
            customer.CreditBalance.Amount += existingPayment.Amount.Amount;
            //لازم ارجع القيمه الي كان دافعها في الاول عشان ابدا اتعامل مع الجديد علي نضافه
            // Apply new payment effect
            customer.CreditBalance.Amount -= paymentDto.Amount.Amount;
            if (customer.CreditBalance.Amount < 0)
                customer.CreditBalance.Amount = 0;

            // Update Invoice Status
            if (paymentDto.Status == PaymentStatus.Completed)
            {
                invoice.Status = InvoiceStatus.Paid;
            }
            else if (paymentDto.Status == PaymentStatus.Pending)
            {
                invoice.Status = InvoiceStatus.Issued;
            }


            existingPayment.InvoiceId = paymentDto.InvoiceId;
            existingPayment.CustomerId = paymentDto.CustomerId;
            existingPayment.Amount = _mapper.Map<Money>(paymentDto.Amount);
            existingPayment.Method = paymentDto.Method;
            existingPayment.Status = paymentDto.Status;
            existingPayment.TransactionId =
                string.IsNullOrEmpty(paymentDto.TransactionId) ? existingPayment.TransactionId : paymentDto.TransactionId;
            existingPayment.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Invoice.UpdateAsync(invoice);
            await _unitOfWork.Customer.UpdateAsync(customer);
            await _unitOfWork.Payment.UpdateAsync(existingPayment);
            await _unitOfWork.SaveChangesAsync();

            var updatedPayment = await _unitOfWork.Payment.GetByIdAsync(id);
            return _mapper.Map<PaymentResponseDto>(updatedPayment);
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetAllPaymentAsync()
        {
            var payment =await _unitOfWork.Payment.GetAllAsync();
            return _mapper.Map<IEnumerable<PaymentResponseDto>>(payment);
        }
        public async Task<PaymentResponseDto> GetPaymentByIdAsync(int id)
        {
            var payment = await _unitOfWork.Payment.GetByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found.");

            return _mapper.Map<PaymentResponseDto>(payment);
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsByCustomerIdAsync(int customerId)
        {
            var payments = await _unitOfWork.Payment.GetPaymentsByCustomerIdAsync(customerId);
            return _mapper.Map<IEnumerable<PaymentResponseDto>>(payments);
        }
        public async Task<IEnumerable<PaymentResponseDto>> GetPendingPaymentsAsync()
        {
            var payments = await _unitOfWork.Payment.GetPendingPaymentsAsync();
            return _mapper.Map<IEnumerable<PaymentResponseDto>>(payments);
        }
        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsByInvoiceIdAsync(int invoiceId)
        {
            var payments = await _unitOfWork.Payment.GetPaymentsByInvoiceIdAsync(invoiceId);
            return _mapper.Map<IEnumerable<PaymentResponseDto>>(payments);
        }

        public async Task CancelPaymentAsync(int id)
        {
            var payment = await _unitOfWork.Payment.GetByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found.");
            //ارجع المبلغ تاني علي حساب الذبون 
            var customer = await _unitOfWork.Customer.GetByIdAsync(payment.CustomerId);
            if (customer != null && customer.CreditBalance != null)
            {
                customer.CreditBalance.Amount += payment.Amount.Amount;
                await _unitOfWork.Customer.UpdateAsync(customer);
            }
            // Revert Invoice Status if necessary
            var invoice = await _unitOfWork.Invoice.GetByIdAsync(payment.InvoiceId);
            if (invoice != null)
            {
                invoice.Status = InvoiceStatus.Issued;
                await _unitOfWork.Invoice.UpdateAsync(invoice);
            }
            await _unitOfWork.Payment.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

    }
}
