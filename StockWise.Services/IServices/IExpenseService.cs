using StockWise.Services.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseDto>> GetAllExpenseAsync();
        Task<ExpenseDto> GetExpenseByIdAsync(int id);
        Task UpdateExpenseAsync(ExpenseDto expenseDto);
        Task CreateExpenseAsync(ExpenseDto expenseDto);
        Task DeleteExpenseAsync(int id);
    }
}
