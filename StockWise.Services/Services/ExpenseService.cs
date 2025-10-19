using AutoMapper;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.ExpenseDto;
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
        private readonly IMapper _mapper;
        public ExpenseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            
        }
        public async Task<ExpenseResponseDto> CreateExpenseAsync(ExpenseCreateDto expenseDto)
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
            var Expen = _mapper.Map<Expense>(expenseDto);
            Expen.CreatedAt = DateTime.UtcNow;
            Expen.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Expense.AddAsync(Expen);
            await _unitOfWork.SaveChangesAsync();
            var CreatedExpense = _unitOfWork.Expense.GetByIdAsync(Expen.Id);
            return _mapper.Map<ExpenseResponseDto>(CreatedExpense);
        }

        public async Task DeleteExpenseAsync(int id)
        {
            var expense = await _unitOfWork.Expense.GetByIdAsync(id);
            if (expense == null)
                throw new KeyNotFoundException($"Expense with ID {id} not found.");
            
            await _unitOfWork.Expense.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ExpenseResponseDto>> GetAllExpenseAsync()
        {
            var expenses = await _unitOfWork.Expense.GetAllAsync();
            return _mapper.Map<IEnumerable<ExpenseResponseDto>>(expenses);
        }

        public async Task<ExpenseResponseDto> GetExpenseByIdAsync(int id)
        {
            var expense = await _unitOfWork.Expense.GetByIdAsync(id);
            if (expense == null)
                throw new KeyNotFoundException($"Expense with ID {id} not found.");

            return _mapper.Map<ExpenseResponseDto>(expense);
        }

        public async Task<IEnumerable<ExpenseResponseDto>> GetExpensesByRepresentativeIdAsync(int representativeId)
        {
            var representative = await _unitOfWork.Representatives.GetByIdAsync(representativeId);
            if (representative == null)
                throw new KeyNotFoundException($"Representative with ID {representativeId} not found.");

            var expenses = await _unitOfWork.Expense.GetByRepresentativeIdAsync(representativeId);
            return _mapper.Map<IEnumerable<ExpenseResponseDto>>(expenses);
        }

        public async Task<ExpenseResponseDto> UpdateExpenseAsync(int id,ExpenseCreateDto expenseDto)
        {
            if (expenseDto == null)
                throw new ArgumentNullException(nameof(expenseDto));

            var existingExpense = await _unitOfWork.Expense.GetByIdAsync(id);
            if (existingExpense == null)
                throw new KeyNotFoundException($"Expense with ID {id} not found.");
            

            if (expenseDto.RepresentativeId.HasValue)
            {
                var representative = await _unitOfWork.Representatives.GetByIdAsync(expenseDto.RepresentativeId.Value);
                if (representative == null)
                    throw new BusinessException("Representative not found.");
            }
            _mapper.Map(expenseDto, existingExpense);
            existingExpense.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Expense.UpdateAsync(existingExpense);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ExpenseResponseDto>(existingExpense);
        }
        
    }
}

