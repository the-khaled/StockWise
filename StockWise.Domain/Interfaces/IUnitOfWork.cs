using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }
        IWarehouseRepository Warehouses { get; }
        IStockRepository Stocks { get; }
        IRepresentativeRepository Representatives { get; }
        ITransferRepository Transfers { get; }


        Task SaveChangesAsync();
    }
}
