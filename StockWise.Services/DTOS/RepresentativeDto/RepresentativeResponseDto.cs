using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS.RepresentativeDto
{
    public class RepresentativeResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NationalId { get; set; }
        public ICollection<string> PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }
        public int WarehouseId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
