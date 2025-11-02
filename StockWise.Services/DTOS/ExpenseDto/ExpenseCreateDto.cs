using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS.ExpenseDto
{
    public class ExpenseCreateDto
    {
        [Required(ErrorMessage = "Amount is required.")]
        public MoneyDto Amount { get; set; }

        [Required(ErrorMessage = "ExpenseType is required.")]
        public Domain.Enums.ExpenseType? ExpenseType { get; set; }

        public string? Description { get; set; }

        public int? RepresentativeId { get; set; }
    }
}
