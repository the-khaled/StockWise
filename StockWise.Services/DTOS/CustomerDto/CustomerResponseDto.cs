using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS.CustomerDto
{
    public class CustomerResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> PhoneNumbers { get; set; }
        public string Address { get; set; }
        public MoneyDto CreditBalance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
