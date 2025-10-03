using StockWise.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Domain.Models
{
    public class Product:BaseEntity
    {
        public string Name { get; set; }
        public Money Price { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public ProductCondition Condition { get; set; } = ProductCondition.Good;

        public ICollection<Stock> stocks { get; set; } = new List<Stock>();
        public ICollection<InvoiceItem> invoiceItems { get; set; }= new List<InvoiceItem>();
        public ICollection<Return> returns { get; set; } = new List<Return>();
        public ICollection<Transfer> transfers { get; set; } = new List<Transfer>();
    }
}
