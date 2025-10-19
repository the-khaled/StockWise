﻿using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StockWise.Domain.Enums.Enums;

namespace StockWise.Services.DTOS.ReturnDto
{
    public class ReturnCreateDto
    {
        [Required(ErrorMessage = "ReturnType is required.")]
        public ReturnType ReturnType { get; set; }
        [Required(ErrorMessage = "ProductId is required.")]
        public int ProductId { get; set; }
        public int? RepresentativeId { get; set; }
        public int? CustomerId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }
        public ProductCondition Condition { get; set; }
        public string Reason { get; set; }
      
    }
}
