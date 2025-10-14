using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Services.DTOS.WarehouseDto
{
    public class WarehouseCreateDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "WarehouseType is required.")]
        public WarehouseType WarehouseType { get; set; }
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }
    }
}
