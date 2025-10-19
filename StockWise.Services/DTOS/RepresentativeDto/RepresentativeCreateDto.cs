using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS.RepresentativeDto
{
    public class RepresentativeCreateDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(14, ErrorMessage = "NationalId cannot exceed 14 characters.")]
        public string NationalId { get; set; }

        [Required(ErrorMessage = "At least one phone number is required.")]
        public ICollection<string> PhoneNumber { get; set; } = new List<string>();

        public string Address { get; set; }
        public string Notes { get; set; }

        [Required(ErrorMessage = "WarehouseId is required.")]
        public int WarehouseId { get; set; }
    }
}
