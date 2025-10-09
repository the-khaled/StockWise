using Microsoft.EntityFrameworkCore;
using StockWise.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Services.DTOS
{
    public class ExpenseDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Amount is required.") ]
        [Precision(18, 2)]
        public Money Amount { get; set; }

        [Required(ErrorMessage = "ExpenseType is required.")]
        public ExpenseType ExpenseType { get; set; }
        public string? Description { get; set; }
        public int? RepresentativeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
