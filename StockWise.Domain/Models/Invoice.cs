using StockWise.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Models
{
    public class Invoice :BaseEntity
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int RepresentativeId { get; set; }
        public Representative Representative { get; set; }
        public DateTime Date { get; set; }
        public Money TotalAmount { get; set; }

        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        /* public ICollection<Item> items { get; set; }
         public ICollection<Payment> payments { get; set; }*/
    }
}
