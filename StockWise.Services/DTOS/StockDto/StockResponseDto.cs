using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS.StockDto
{
    public class StockResponseDto
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
      
        public string WarehouseName { get; set; }
        public int ProductId { get; set; }
       
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int MinQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

