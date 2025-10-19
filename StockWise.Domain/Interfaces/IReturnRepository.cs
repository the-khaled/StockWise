using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Interfaces
{
    public interface IReturnRepository:IRepository<Return>
    {
        Task<IEnumerable<Return>> GetByProductIdAsync(int productId);
        Task<IEnumerable<Return>> GetByRepresentativeIdAsync(int representativeId);
        Task<IEnumerable<Return>> GetByCustomerIdAsync(int customerId);
    }
}
