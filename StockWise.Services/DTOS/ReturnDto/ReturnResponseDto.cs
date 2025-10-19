using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Services.DTOS.ReturnDto
{
    public class ReturnResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? RepresentativeId { get; set; }
        public string RepresentativeName { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int Quantity { get; set; }
        public ReturnType ReturnType { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
