using StockWise.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Services.DTOS
{
    public class ProductDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "ProductionDate is required.")]
        public DateTime ProductionDate { get; set; }
        [Required(ErrorMessage = "ExpiryDate is required.")]
        public DateTime ExpiryDate { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        public Money Price { get; set; }
        [Required(ErrorMessage = "Condition is required.")]
        public ProductCondition Condition { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
