using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Domain.Models
{
    public class Return:BaseEntity
    {
        public ReturnType ReturnType { get; set; } // FromRepresentative or FromCustomer
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public ProductCondition Condition { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public int? RepresentativeId { get; set; }
        public Representative Representative { get; set; }
        public int? CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
