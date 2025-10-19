using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IInvoiceRepository Invoice { get; }
        IInvoiceItemRepository InvoiceItem { get; }
        IExpenseRepository Expense { get; }
        IPaymentRepository Payment { get; }
        ILocationRepository Location { get; }
        IReturnRepository Return { get; }
        ICustomerRepository Customer { get; }
        IProductRepository Products { get; }
        IWarehouseRepository Warehouses { get; }
        IStockRepository Stocks { get; }
        IRepresentativeRepository Representatives { get; }
        ITransferRepository Transfers { get; }
        IDamagedProductRepositories DamagedProduct { get; }


        Task<int> SaveChangesAsync();
    }
}
