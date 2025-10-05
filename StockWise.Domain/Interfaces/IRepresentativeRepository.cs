using StockWise.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Domain.Interfaces
{
    public interface IRepresentativeRepository
    {
        Task<Representative> GetByIdAsync(int id);
        Task<IEnumerable<Representative>> GetAllAsync();
        Task AddAsync(Representative representative);
        Task UpdateAsync(Representative representative);
        Task DeleteAsync(int id);
    }
}
