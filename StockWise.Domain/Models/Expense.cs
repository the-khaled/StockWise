using StockWise.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Models
{
    public class Expense:BaseEntity
    {
        public string? Description { get; set; }
        public Money Amount { get; set; }
        public Enums.ExpenseType ExpenseType { get; set; } = Enums.ExpenseType.General;
        public int? RepresentativeId { get; set; }
        public Representative Representative { get; set; }
    }
}

