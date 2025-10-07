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
    public class UnitOfWork:IUnitOfWork
    {
        private readonly StockWiseDbContext _context;
        public IProductRepository Products { get; }
        public IWarehouseRepository Warehouses { get;  }
        public IStockRepository Stocks { get; }
        public IRepresentativeRepository Representatives { get; }
        public ITransferRepository Transfers { get; }

        public IInvoiceRepository Invoice { get; }

        public IInvoiceItemRepository InvoiceItem { get; }

        public IExpenseRepository expense { get; }  

        public IPaymentRepository Payment { get; }

        public ILocationRepository Location { get; }

        public IReturnRepository Return { get; }

        public ICustomerRepository Customer { get; }

        public UnitOfWork(StockWiseDbContext context)
        {
            _context = context;   
            Products= new ProductRepository(context);
            Warehouses=new WarehouseRepository(context);
            Stocks = new StockRepository(context);
            Transfers = new TransferRepository(context);
            Representatives=new RepresentativeRepository(context);
/*            Invoice =new IInvoiceRepository(context);
*/        }
        public async Task SaveChangesAsync() 
        {
            await _context.SaveChangesAsync();
        }
    }
}
