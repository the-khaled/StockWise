using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Services.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ExpenseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreateExpenseAsync(ExpenseDto expenseDto)
        {
            if (expenseDto == null)
                throw new ArgumentNullException(nameof(expenseDto));

            if (expenseDto.Amount.Amount < 0)
                throw new BusinessException("Expense amount cannot be negative.");

            if (expenseDto.RepresentativeId != null)
            {
                var representative = await _unitOfWork.Representatives.GetByIdAsync(expenseDto.RepresentativeId.Value);
                if (representative == null)
                    throw new BusinessException("Representative not found.");
            }

            await _unitOfWork.Expense.AddAsync(MapToEntity(expenseDto));
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteExpenseAsync(int id)
        {
            var expense = await _unitOfWork.Expense.GetByIdAsync(id);
            if (expense == null)
                throw new KeyNotFoundException($"Expense with ID {id} not found.");
            
            await _unitOfWork.Expense.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ExpenseDto>> GetAllExpenseAsync()
        {
            var expenses = await _unitOfWork.Expense.GetAllAsync();
            return expenses.Select(e => MapToDto(e)).ToList();
        }

        public async Task<ExpenseDto> GetExpenseByIdAsync(int id)
        {
            var expense = await _unitOfWork.Expense.GetByIdAsync(id);
            if (expense == null)
                throw new KeyNotFoundException($"Expense with ID {id} not found.");

            return MapToDto(expense);
        }

        public async Task UpdateExpenseAsync(ExpenseDto expenseDto)
        {
            if (expenseDto == null)
                throw new ArgumentNullException(nameof(expenseDto));

            var existingExpense = await _unitOfWork.Expense.GetByIdAsync(expenseDto.Id);
            if (existingExpense == null)
                throw new KeyNotFoundException($"Expense with ID {expenseDto.Id} not found.");
            

            if (expenseDto.RepresentativeId.HasValue)
            {
                var representative = await _unitOfWork.Representatives.GetByIdAsync(expenseDto.RepresentativeId.Value);
                if (representative == null)
                    throw new BusinessException("Representative not found.");
            }

            existingExpense.Amount = expenseDto.Amount;
            existingExpense.ExpenseType = expenseDto.ExpenseType;
            existingExpense.RepresentativeId = expenseDto.RepresentativeId;
            existingExpense.UpdatedAt = DateTime.Now;

            await _unitOfWork.Expense.UpdateAsync(existingExpense);
            await _unitOfWork.SaveChangesAsync();
        }
        private ExpenseDto MapToDto(Expense expense)
        {
            return new ExpenseDto
            {
                Id = expense.Id,
                Amount = expense.Amount,
                ExpenseType = expense.ExpenseType,
                RepresentativeId = expense.RepresentativeId,
                CreatedAt = expense.CreatedAt,
                UpdatedAt = expense.UpdatedAt
            };
        }

        private Expense MapToEntity(ExpenseDto dto)
        {
            return new Expense
            {
                Id = dto.Id,
                Amount = dto.Amount,
                ExpenseType = dto.ExpenseType,
                RepresentativeId = dto.RepresentativeId,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }
}

