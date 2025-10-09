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
    public class PaymentDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "InvoiceId is required.")]
        public int InvoiceId { get; set; }

        [Required(ErrorMessage = "CustomerId is required.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        public Money Amount { get; set; }

        [Required(ErrorMessage = "Method is required.")]
        public PaymentMethod Method { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public PaymentStatus Status { get; set; }
      
        public string TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
