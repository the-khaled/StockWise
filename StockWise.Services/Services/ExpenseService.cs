using StockWise.Domain.Interfaces;
using StockWise.Services.DTOS;
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
        public Task CreateExpenseAsync(ExpenseDto expenseDto)
        {
            throw new NotImplementedException();
        }

        public Task DeleteExpenseAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ExpenseDto>> GetAllExpenseAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ExpenseDto> GetExpenseByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateExpenseAsync(ExpenseDto expenseDto)
        {
            throw new NotImplementedException();
        }

    }
}
