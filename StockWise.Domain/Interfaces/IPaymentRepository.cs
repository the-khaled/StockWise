using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Interfaces
{
    public interface IPaymentRepository:IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByInvoiceIdAsync(int invoiceId);
        Task<IEnumerable<Payment>> GetPaymentsByCustomerIdAsync(int customerId);
         Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
    }
}
