using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Domain.Models
{
    public class Warehouse:BaseEntity
    {
        public string Name { get; set; }
        public WarehouseType WarehouseType { get; set; }
        public string Address { get; set; }
        public ICollection<Representative> Representatives { get; set; }=new List<Representative>();
        public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        public ICollection<Transfer> TransfersFrom { get; set; } = new List<Transfer>();
        public ICollection<Transfer> TransfersTo { get; set; } =new List<Transfer>();

    }
}
