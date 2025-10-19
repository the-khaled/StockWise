using StockWise.Services.DTOS;
using StockWise.Services.DTOS.ExpenseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseResponseDto>> GetAllExpenseAsync();
        Task<ExpenseResponseDto> GetExpenseByIdAsync(int id);
        Task<ExpenseResponseDto> UpdateExpenseAsync(int id,ExpenseCreateDto expenseDto);
        Task<ExpenseResponseDto> CreateExpenseAsync(ExpenseCreateDto expenseDto);
        Task<IEnumerable<ExpenseResponseDto>> GetExpensesByRepresentativeIdAsync(int representativeId);
        Task DeleteExpenseAsync(int id);
    }
}
