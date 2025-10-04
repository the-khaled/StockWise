using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Enums
{
    public class Enums
    {
        public enum WarehouseType { Main, Branch }
        public enum ProductCondition { Good, Damaged }
        public enum PaymentMethod { Cash, Electronic }
        public enum PaymentStatus { Pending, Completed, Failed }
        public enum ReturnType { FromRepresentative, FromCustomer }
        public enum ExpenseType { General, Advance, Fuel, Rent, Maintenance } 
    }
}
