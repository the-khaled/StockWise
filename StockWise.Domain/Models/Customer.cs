using StockWise.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Models
{
    public class Customer:BaseEntity
    {
        public string Name { get; set; }
        public ICollection<string> PhoneNumbers { get; set; } = new List<string>();
        public string? Address { get; set; }
        public Money CreditBalance { get; set; } 
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Return> Returns { get; set; } = new List<Return>();
    }
}
