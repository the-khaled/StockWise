using Microsoft.EntityFrameworkCore;
using StockWise.Domain.Enums;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Infrastructure.Repositories
{
    public class PaymentRepository:GenericRepository<Payment>,IPaymentRepository
    {
        public PaymentRepository(StockWiseDbContext context) : base(context) { }
        
            public async Task<IEnumerable<Payment>> GetPaymentsByInvoiceIdAsync(int invoiceId)
            {
                return await _context.payments
                    .Where(p => p.InvoiceId == invoiceId)
                    .ToListAsync();
            }

            public async Task<IEnumerable<Payment>> GetPaymentsByCustomerIdAsync(int customerId)
            {
                return await _context.payments
                    .Where(p => p.CustomerId == customerId)
                    .ToListAsync();
            }

            public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
            {
                return await _context.payments
                    .Where(p => p.Status == Enums.PaymentStatus.Pending)
                    .ToListAsync();
            }
        }
    }
