using StockWise.Domain.Interfaces;
using StockWise.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Infrastructure.Repositories
{
    public class UnitOfWork:IUnitOfWork
    {
        private readonly StockWiseDbContext _context;
        public IProductRepository Products { get; }
        public IWarehouseRepository Warehouses { get;  }
        public UnitOfWork(StockWiseDbContext context)
        {
            _context = context;   
            Products= new ProductRepository(context);
            Warehouses=new WarehouseRepository(context);
        }
        public async Task SaveChangesAsync() 
        {
            await _context.SaveChangesAsync();
        }
    }
}
