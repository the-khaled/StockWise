using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.PaymentDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentResponseDto>> GetAllPaymentAsync();
        Task<PaymentResponseDto> GetPaymentByIdAsync(int id);
        Task<PaymentResponseDto> UpdatePaymentAsync(int id,PaymentCreateDto paymentDto);
        Task<PaymentResponseDto> CreatePaymentAsync(PaymentCreateDto paymentDto);
        Task<IEnumerable<PaymentResponseDto>> GetPaymentsByInvoiceIdAsync(int invoiceId);
        Task<IEnumerable<PaymentResponseDto>> GetPaymentsByCustomerIdAsync(int customerId);
        Task<IEnumerable<PaymentResponseDto>> GetPendingPaymentsAsync();
        Task CancelPaymentAsync(int id);
    }
}
