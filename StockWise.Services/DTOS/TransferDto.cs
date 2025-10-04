using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS
{
    public class TransferDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "FromWarehouseId is required.")]
        public int FromWarehouseId { get; set; }

        [Required(ErrorMessage = "ToWarehouseId is required.")]
        public int ToWarehouseId { get;set; }
        [Required(ErrorMessage = "ProductId is required.")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; }

    }
}
