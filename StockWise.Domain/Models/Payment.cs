using StockWise.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Models
{
    public class Payment:BaseEntity
    {
        public Invoice Invoice { get; set; }
        public int InvoiceId { get; set; }
        public Customer Customer { get; set; }
        public int CustomerId { get; set; }
        public Money Amount { get; set; }
        public Enums.PaymentMethod Method { get; set; } // Cash or Electronic
        public Enums.PaymentStatus Status { get; set; } = Enums.PaymentStatus.Pending;
        public string TransactionId { get; set; }
    }
}
