using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Domain.ValueObjects;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.PaymentDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<GenericResponse<PaymentResponseDto>> CreatePaymentAsync(PaymentCreateDto paymentDto)
        {
            var respons = new GenericResponse<PaymentResponseDto>();
            if (paymentDto == null)
            {
                respons.StatusCode =(int)HttpStatusCode.BadRequest;
                respons.Success= false;
                respons.Message = "Payment data is required.";
                return respons;
            }

            if (paymentDto.Amount.Amount < 0 || paymentDto.Amount == null) 
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Payment amount cannot be negative.";
                return respons;
            }
               

            var invoice = await _unitOfWork.Invoice.GetByIdAsync(paymentDto.InvoiceId);
            if (invoice == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = "Invoice not found.";
                return respons;
            }

            var customer = await _unitOfWork.Customer.GetByIdAsync(paymentDto.CustomerId);
            if (customer == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = "Customer not found.";
                return respons;
            }
            if (string.IsNullOrEmpty(paymentDto.TransactionId))
                paymentDto.TransactionId = Guid.NewGuid().ToString();
            if (invoice.CustomerId != paymentDto.CustomerId)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = $"Invoice {paymentDto.InvoiceId} is not associated with Customer {paymentDto.CustomerId}.";
                return respons;
            }
            if (customer.CreditBalance == null)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = $"CreditBalance is not initialized for Customer ID {paymentDto.CustomerId}.";
                return respons;
            }
            //////////////////////////////////////
            if (paymentDto.Amount.Currency != customer.CreditBalance.Currency)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = $"Currency mismatch: Payment ({paymentDto.Amount.Currency}) does not match Customer CreditBalance ({customer.CreditBalance.Currency}).";
                return respons;
            }
            //////////////////////////////////
            var paymentEntity = _mapper.Map<Payment>(paymentDto);
            paymentEntity.CreatedAt = DateTime.UtcNow;
            paymentEntity.UpdatedAt = DateTime.UtcNow;
            paymentEntity.TransactionId =
                string.IsNullOrEmpty(paymentDto.TransactionId) ? Guid.NewGuid().ToString() : paymentDto.TransactionId;
            // Update Invoice Status
            if (paymentDto.Status == Domain.Enums.PaymentStatus.Completed)
            {
                invoice.Status = Domain.Enums.InvoiceStatus.Paid;
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

            respons.StatusCode = (int)HttpStatusCode.Created;
            respons.Success = true;
            respons.Message = "Success";
            respons.Data = _mapper.Map<PaymentResponseDto>(createdPayment);
            return respons;
        }
        public async Task<GenericResponse<PaymentResponseDto>> UpdatePaymentAsync(int id,PaymentCreateDto paymentDto)
        {
            var respons= new GenericResponse<PaymentResponseDto>();
            if (paymentDto == null)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success= false;
                respons.Message = "data is required.";
                return respons;
            }
            if (paymentDto.Amount == null || paymentDto.Amount.Amount <= 0)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Payment amount must be greater than zero.";
                return respons;
            }

            var existingPayment = await _unitOfWork.Payment.GetByIdAsync(id);
            if (existingPayment == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Payment with ID {id} not found.";
                return respons;
            }
            var invoice = await _unitOfWork.Invoice.GetByIdAsync(paymentDto.InvoiceId);
            if (invoice == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Invoice with ID {paymentDto.InvoiceId} not found.";
                return respons;
            }

            var customer = await _unitOfWork.Customer.GetByIdAsync(paymentDto.CustomerId);
            if (customer == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Customer with ID {paymentDto.CustomerId} not found.";
                return respons;
            }

            if (invoice.CustomerId != paymentDto.CustomerId)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = $"Invoice {paymentDto.InvoiceId} is not associated with Customer {paymentDto.CustomerId}.";
                return respons;
            }

            if (customer.CreditBalance == null)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = $"CreditBalance is not initialized for Customer ID {paymentDto.CustomerId}.";
                return respons;
            }

            if (paymentDto.Amount.Currency != customer.CreditBalance.Currency)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = $"Currency mismatch: Payment ({paymentDto.Amount.Currency}) does not match Customer CreditBalance ({customer.CreditBalance.Currency}).";
                return respons;
            }
            // Revert previous payment effect on CreditBalance
            customer.CreditBalance.Amount += existingPayment.Amount.Amount;
            //لازم ارجع القيمه الي كان دافعها في الاول عشان ابدا اتعامل مع الجديد علي نضافه
            // Apply new payment effect
            customer.CreditBalance.Amount -= paymentDto.Amount.Amount;
            if (customer.CreditBalance.Amount < 0)
                customer.CreditBalance.Amount = 0;

            // Update Invoice Status
            if (paymentDto.Status == Domain.Enums.PaymentStatus.Completed)
            {
                invoice.Status = Domain.Enums.InvoiceStatus.Paid;
            }
            else if (paymentDto.Status == Domain.Enums.PaymentStatus.Pending)
            {
                invoice.Status = Domain.Enums.InvoiceStatus.Issued;
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

            respons.StatusCode=(int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "Success";
            respons.Data= _mapper.Map<PaymentResponseDto>(updatedPayment);
            return  respons;
        }

        public async Task<GenericResponse<IEnumerable<PaymentResponseDto>>> GetAllPaymentAsync()
        {
            var respons = new GenericResponse<IEnumerable<PaymentResponseDto>>();

            var payment =await _unitOfWork.Payment.GetAllAsync();
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "Success";
            respons.Data = _mapper.Map<IEnumerable<PaymentResponseDto>>(payment);
            return respons;
        }
        public async Task<GenericResponse<PaymentResponseDto>> GetPaymentByIdAsync(int id)
        {
            var respons = new GenericResponse<PaymentResponseDto>();

            var payment = await _unitOfWork.Payment.GetByIdAsync(id);
            if (payment == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Payment with ID {id} not found.";
                return respons;
            }
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "Success";
            respons.Data = _mapper.Map<PaymentResponseDto>(payment);
            return respons;
        }

        public async Task<GenericResponse<IEnumerable<PaymentResponseDto>>> GetPaymentsByCustomerIdAsync(int customerId)
        {
            var respons = new GenericResponse<IEnumerable<PaymentResponseDto>>();

            var payments = await _unitOfWork.Payment.GetPaymentsByCustomerIdAsync(customerId);
            if (payments == null) 
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"there is no Customer with id {customerId} ";
                return respons;
            }
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "Success";
            respons.Data = _mapper.Map<IEnumerable<PaymentResponseDto>>(payments);
            return respons;
        }
        public async Task<GenericResponse<IEnumerable<PaymentResponseDto>>> GetPendingPaymentsAsync()
        {
            var respons = new GenericResponse<IEnumerable<PaymentResponseDto>>();

            var payments = await _unitOfWork.Payment.GetPendingPaymentsAsync();
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "Success";
            respons.Data = _mapper.Map<IEnumerable<PaymentResponseDto>>(payments);
            return respons ;
        }
        public async Task<GenericResponse<IEnumerable<PaymentResponseDto>>> GetPaymentsByInvoiceIdAsync(int invoiceId)
        {
            var respons = new GenericResponse<IEnumerable<PaymentResponseDto>>();
            var payments = await _unitOfWork.Payment.GetPaymentsByInvoiceIdAsync(invoiceId);

            if (payments == null||!payments.Any()) 
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"No payments found for invoice ID {invoiceId}.";
                return respons;

            }
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "Success";
            respons.Data = _mapper.Map<IEnumerable<PaymentResponseDto>>(payments);
            return respons;
        }

        public async Task<GenericResponse<PaymentResponseDto>> CancelPaymentAsync(int id)
        {
            var respons= new GenericResponse<PaymentResponseDto>();
            var payment = await _unitOfWork.Payment.GetByIdAsync(id);
            if (payment == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Success = false;
                respons.Message = $"Payment with ID {id} not found.";
                return respons;
            }
            //ارجع المبلغ تاني علي حساب الذبون 
            var customer = await _unitOfWork.Customer.GetByIdAsync(payment.CustomerId);
            if (customer != null && customer.CreditBalance != null)
            {
                customer.CreditBalance.Amount += payment.Amount.Amount;
                await _unitOfWork.Customer.UpdateAsync(customer);
            }
            // Revert Invoice Status 
            var invoice = await _unitOfWork.Invoice.GetByIdAsync(payment.InvoiceId);
            if (invoice != null)
            {
                invoice.Status = Domain.Enums.InvoiceStatus.Issued;
                await _unitOfWork.Invoice.UpdateAsync(invoice);
            }
            await _unitOfWork.Payment.Cancel(id);
            await _unitOfWork.SaveChangesAsync();
            respons.StatusCode = (int)HttpStatusCode.NoContent;
            respons.Success = true;
            respons.Message = "Payment has been canceled successfully";
            return respons;
        }

    }
}
