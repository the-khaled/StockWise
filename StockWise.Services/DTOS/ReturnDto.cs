using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Services.DTOS
{
    public class ReturnDto
    {
        public int Id { get; set; } 
        [Required(ErrorMessage = "ProductId is required.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "RepresentativeId is required.")]
        public int? RepresentativeId { get; set; }

        [Required(ErrorMessage = "CustomerId is required.")]
        public int? CustomerId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "ReturnType is required.")]
        public ReturnType ReturnType { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
