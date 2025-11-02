using StockWise.Services.DTOS;
using StockWise.Services.DTOS.ExpenseDto;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.IServices
{
    public interface IExpenseService
    {
        Task<GenericResponse<IEnumerable<ExpenseResponseDto>>> GetAllExpenseAsync();
        Task<GenericResponse<ExpenseResponseDto>> GetExpenseByIdAsync(int id);
        Task<GenericResponse<ExpenseResponseDto>> UpdateExpenseAsync(int id,ExpenseCreateDto expenseDto);
        Task<GenericResponse<ExpenseResponseDto>> CreateExpenseAsync(ExpenseCreateDto expenseDto);
        Task<GenericResponse<IEnumerable<ExpenseResponseDto>>> GetExpensesByRepresentativeIdAsync(int representativeId);
        Task<GenericResponse<ExpenseResponseDto>> DeleteExpenseAsync(int id);
    }
}
