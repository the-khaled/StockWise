using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Models
{
    public class Representative:BaseEntity
    {
        public string Name {  get; set; }
        public string NationalId { get; set; }
        public ICollection<string> PhoneNumber { get; set; } = new List<string>();
        public string Address { get; set; } 
        public string Notes { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<Return> Returns { get; set; } = new List<Return>(); // Returns from rep to main
        public ICollection<Location> Locations { get; set; } = new List<Location>();
        public ICollection<Expense> Expenses { get; set; }

    }
}
