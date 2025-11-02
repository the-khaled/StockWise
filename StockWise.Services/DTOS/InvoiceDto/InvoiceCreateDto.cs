using StockWise.Services.DTOS.InvoiceItemDto;
using StockWise.Services.DTOS.InvoiceItemDto.InvoiceItemDto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS.InvoiceDto
{
    public class InvoiceCreateDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int RepresentativeId { get; set; }

        [Required]
        public Domain.Enums.InvoiceStatus Status { get; set; }

      
        [Required]
        [MinLength(1)]
        public List<InvoiceItemCreateDto> Items { get; set; } = new List<InvoiceItemCreateDto>();
    }
}
