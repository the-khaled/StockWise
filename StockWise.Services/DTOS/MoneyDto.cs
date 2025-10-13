using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS
{
    public class MoneyDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
    }
}
