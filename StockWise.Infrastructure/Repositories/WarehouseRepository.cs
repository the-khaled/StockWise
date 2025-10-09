using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Infrastructure.Repositories
{
    public class WarehouseRepository:GenericRepository<Warehouse>,IWarehouseRepository
    {
        public WarehouseRepository(StockWiseDbContext context) : base(context) { }
    }
}
