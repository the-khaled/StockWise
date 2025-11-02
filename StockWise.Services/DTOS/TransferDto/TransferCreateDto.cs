using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS.TransferDto
{
    public class TransferCreateDto
    {
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int ProductId { get; set; }
        [Range(1,200)]
        public int Quantity { get; set; }
    }
}
