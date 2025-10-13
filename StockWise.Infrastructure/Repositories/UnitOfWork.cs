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

    public class UnitOfWork : IUnitOfWork , IDisposable
    {
        private readonly StockWiseDbContext _context;

        private IProductRepository _products;
        private IWarehouseRepository _warehouses;
        private IStockRepository _stocks;
        private IRepresentativeRepository _representatives;
        private ITransferRepository _transfers;
        private IInvoiceRepository _invoices;
        private IInvoiceItemRepository _invoiceItems;
        private IExpenseRepository _expenses;
        private IPaymentRepository _payments;
        private ILocationRepository _locations;
        private IReturnRepository _returns;
        private ICustomerRepository _customers;

        public UnitOfWork(StockWiseDbContext context)
        {
            _context = context;
        }
        //        علشان نحافظ على الأداء وما نكررش إنشاء Repositories كتير
        public IProductRepository Products => _products ??= new ProductRepository(_context);
        public IWarehouseRepository Warehouses => _warehouses ??= new WarehouseRepository(_context);
        public IStockRepository Stocks => _stocks ??= new StockRepository(_context);
        public IRepresentativeRepository Representatives => _representatives ??= new RepresentativeRepository(_context);
        public ITransferRepository Transfers => _transfers ??= new TransferRepository(_context);
        public IInvoiceRepository Invoice => _invoices ??= new InvoiceRepository(_context);
        public IInvoiceItemRepository InvoiceItem => _invoiceItems ??= new InvoiceItemRepository(_context);
        public IExpenseRepository expense => _expenses ??= new ExpenseRepository(_context);
        public IPaymentRepository Payment => _payments ??= new PaymentRepository(_context);
        public ILocationRepository Location => _locations ??= new LocationRepository(_context);
        public IReturnRepository Return => _returns ??= new ReturnRepository(_context);
        public ICustomerRepository Customer => _customers ??= new CustomerRepository(_context);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();//تنظيف الموارد وإغلاق الـ DbContext


    }
    /*public class UnitOfWork:IUnitOfWork
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
            Customer =new CustomerRepository(context);
            Products= new ProductRepository(context);
            Warehouses=new WarehouseRepository(context);
            Stocks = new StockRepository(context);
            Transfers = new TransferRepository(context);
            Representatives=new RepresentativeRepository(context);
            Invoice = new InvoiceRepository(context);
            InvoiceItem = new InvoiceItemRepository(context);
            expense = new ExpenseRepository(context);
            Payment = new PaymentRepository(context);
            Location = new LocationRepository(context);
            Return = new ReturnRepository(context);
        }
        public async Task SaveChangesAsync() 
        {
            await _context.SaveChangesAsync();
        }
    }*/
}
