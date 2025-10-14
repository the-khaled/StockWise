using StockWise.Domain.ValueObjects;
using StockWise.Services.DTOS.InvoiceItemDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Services.DTOS.InvoiceDto
{
    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int RepresentativeId { get; set; }
        public string RepresentativeName { get; set; } 
        public MoneyDto TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public List<InvoiceItemResponseDto> Items { get; set; } = new List<InvoiceItemResponseDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
