using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.PaymentDto;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IPaymentService
    {
        Task<GenericResponse<IEnumerable<PaymentResponseDto>>> GetAllPaymentAsync();
        Task<GenericResponse<PaymentResponseDto>> GetPaymentByIdAsync(int id);
        Task<GenericResponse<PaymentResponseDto>> UpdatePaymentAsync(int id,PaymentCreateDto paymentDto);
        Task<GenericResponse<PaymentResponseDto>> CreatePaymentAsync(PaymentCreateDto paymentDto);
        Task<GenericResponse<IEnumerable<PaymentResponseDto>>> GetPaymentsByInvoiceIdAsync(int invoiceId);
        Task<GenericResponse<IEnumerable<PaymentResponseDto>>> GetPaymentsByCustomerIdAsync(int customerId);
        Task<GenericResponse<IEnumerable<PaymentResponseDto>>> GetPendingPaymentsAsync();
        Task<GenericResponse<PaymentResponseDto>> CancelPaymentAsync(int id);
    }
}
