using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS.ExpenseDto
{
    public class ExpenseResponseDto
    {
        public int Id { get; set; }
        public MoneyDto Amount { get; set; }
        public Domain.Enums.ExpenseType ExpenseType { get; set; }
        public string? Description { get; set; }
        public int? RepresentativeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
