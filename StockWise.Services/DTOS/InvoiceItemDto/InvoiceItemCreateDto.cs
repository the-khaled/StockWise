using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS.InvoiceItemDto.InvoiceItemDto
{
    public class InvoiceItemCreateDto
    {
       
        public int? InvoiceId { get; set; }
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public MoneyDto? Price { get; set; }
    }
}
