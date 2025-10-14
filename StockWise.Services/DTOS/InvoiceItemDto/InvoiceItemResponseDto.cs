using StockWise.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS.InvoiceItemDto
{
    public class InvoiceItemResponseDto
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } 
        public int Quantity { get; set; }
        public MoneyDto Price { get; set; }
        public decimal Subtotal { get; set; }
    }
}
