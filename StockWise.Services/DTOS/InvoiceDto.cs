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
    public class InvoiceDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "CustomerId is required.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "RepresentativeId is required.")]
        public int RepresentativeId { get; set; }

       
        public Money? TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }= InvoiceStatus.Draft;
        public List<InvoiceItemDto> Items { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
