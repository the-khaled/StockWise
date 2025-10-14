using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS.StockDto
{
    public class StockCreateDto
    {
        [Required(ErrorMessage = "WarehouseId is required.")]
        public int WarehouseId { get; set; }

        [Required(ErrorMessage = "ProductId is required.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MinQuantity cannot be negative.")]
        public int? MinQuantity { get; set; } = 10;
    }
}
