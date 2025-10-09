using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Models
{
    public class Location:BaseEntity
    {
        public int RepresentativeId { get; set; }
        public Representative Representative { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
