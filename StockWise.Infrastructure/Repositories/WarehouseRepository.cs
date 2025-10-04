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
    internal class WarehouseRepository:GenericRepository<Warehouse>,IWarehouseRepository
    {
        private readonly StockWiseDbContext _Context;
        public WarehouseRepository(StockWiseDbContext context):base(context)
        {
            _Context=context;
        }
    }
}
