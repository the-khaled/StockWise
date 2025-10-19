using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Interfaces
{
    public interface IRepresentativeRepository: IRepository<Representative>
    {
        Task<IEnumerable<Representative>> GetByWarehouseIdAsync(int warehouseId);
        Task<Representative> GetByNationalIdAsync(string nationalId);
    }
}
