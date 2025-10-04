using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Models
{
    public class Transfer:BaseEntity
    {
        public int FromWarehouseId { get; set; }
        public Warehouse FromWarehouse { get; set; }
        public int ToWarehouseId { get; set; }
        public Warehouse ToWarehouse { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}
