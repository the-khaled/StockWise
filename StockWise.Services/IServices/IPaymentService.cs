using StockWise.Services.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetAllPaymentAsync();
        Task<PaymentDto> GetPaymentByIdAsync(int id);
        Task UpdatePaymentAsync(PaymentDto paymentDto);
        Task CreatePaymentAsync(PaymentDto paymentDto);
        Task DeletePaymentAsync(int id);
    }
}
