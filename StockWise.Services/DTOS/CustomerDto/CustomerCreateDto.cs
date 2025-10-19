using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.DTOS.CustomerDto
{
    public class CustomerCreateDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }
        public List<string> PhoneNumbers { get; set; } = new List<string>();
        public string? Address { get; set; }
        [Required(ErrorMessage = "CreditBalance is required.")]
        public MoneyDto CreditBalance { get; set; }
    }
}
