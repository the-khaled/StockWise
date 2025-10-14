using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Services.DTOS.WarehouseDto
{
    public class WarehouseResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public WarehouseType WarehouseType { get; set; }

        public string Address { get; set; }

        public List<int> RepresentativeId { get; set; } = new List<int>();
        public List<int> StockId { get; set; } = new List<int>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
