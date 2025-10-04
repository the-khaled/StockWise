using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.ValueObjects
{
    public class Money
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public Money(decimal amount)
        {
            if (amount < 0) throw new ArgumentException("Amount cannot be negative");
            Amount = amount;
        }
    }
}
