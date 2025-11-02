using Microsoft.AspNetCore.Http;
using StockWise.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Models
{
    public class Product:BaseEntity
    {
        public string Name { get; set; }
        public Money Price { get; set; }
        public string? Imag {  get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? InitialQuantity { get; set; } = 0;
        public Enums.ProductCondition Condition { get; set; } = Enums.ProductCondition.Good;
        public ICollection<Stock> stocks { get; set; } = new List<Stock>();
        public ICollection<InvoiceItem> invoiceItems { get; set; }= new List<InvoiceItem>();
        public ICollection<Return> returns { get; set; } = new List<Return>();
        public ICollection<Transfer> transfers { get; set; } = new List<Transfer>();
    }
}
