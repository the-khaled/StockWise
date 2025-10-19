using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Models
{
    public class DamagedProduct:BaseEntity
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
    }
}
