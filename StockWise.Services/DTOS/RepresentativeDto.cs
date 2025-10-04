using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS
{
    public class RepresentativeDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(14, ErrorMessage = "NationalId cannot exceed 14 characters.")]
        public string NationalId { get; set; }

        public List<string> PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }

        [Required(ErrorMessage = "WarehouseId is required.")]
        public int WarehouseId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
