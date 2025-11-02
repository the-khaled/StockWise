using StockWise.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS.ProductDto
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public MoneyDto Price { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? InitialQuantity { get; set; }
        public Domain.Enums.ProductCondition Condition { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<int> WarehouseIds { get; set; }
        public string? ImageUrl { get; set; }
    }
}
