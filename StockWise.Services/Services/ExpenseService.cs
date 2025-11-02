using AutoMapper;
using Azure;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.ExpenseDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using StockWise.Services.ServicesResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public async Task<GenericResponse<ExpenseResponseDto>> CreateExpenseAsync(ExpenseCreateDto expenseDto)
        {
            var respons = new GenericResponse<ExpenseResponseDto>();
            
            if (expenseDto == null)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Message = "Expense  data is required.";
                respons.Success = false;
                respons.Data = null;
                return respons;
                // throw new ArgumentNullException(nameof(expenseDto));
            }
            if (  !expenseDto.ExpenseType.HasValue || !Enum.IsDefined(typeof(Domain.Enums.ExpenseType), expenseDto.ExpenseType))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Invalid or missing expense type. Allowed values are: General, Advance, Fuel, Rent, Maintenance.";
                respons.Data = null;
                return respons;
            }
            if (expenseDto.Amount.Amount < 0)
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Message = "Expense amount cannot be negative.";
                respons.Success = false;
                respons.Data = null;
                return respons;
                // throw new BusinessException("Expense amount cannot be negative.");
            }
            var allowedCurrencies = new[] { "EGP", "$", "USD", "EUR" };
            expenseDto.Amount.Currency = expenseDto.Amount.Currency?.Trim();

            if (string.IsNullOrWhiteSpace(expenseDto.Amount.Currency) || !allowedCurrencies.Contains(expenseDto.Amount.Currency))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Invalid currency , Allowed values are: EGP , $ , USD , EUR ";
                respons.Data = null;
                return respons;
            }



            if (expenseDto.RepresentativeId != null)
            {
                var representative = await _unitOfWork.Representatives.GetByIdAsync(expenseDto.RepresentativeId.Value);
                if (representative == null)
                {
                    respons.StatusCode = (int)HttpStatusCode.NotFound;
                    respons.Message = "Representative not found.";
                    respons.Success = false;
                    respons.Data = null;
                    return respons;
                    // throw new BusinessException("Representative not found.");
                }
            }
            var Expen = _mapper.Map<Expense>(expenseDto);
            Expen.CreatedAt = DateTime.UtcNow;
            Expen.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Expense.AddAsync(Expen);
            await _unitOfWork.SaveChangesAsync();
            var CreatedExpense = await _unitOfWork.Expense.GetByIdAsync(Expen.Id);
            respons.StatusCode=(int)HttpStatusCode.Created;
            respons.Message = "Successful operation";
            respons.Success= true;
            respons.Data = _mapper.Map<ExpenseResponseDto>(CreatedExpense);
            return respons ;
        }

        public async Task<GenericResponse<ExpenseResponseDto>> DeleteExpenseAsync(int id)
        {
            var respons=new GenericResponse<ExpenseResponseDto>();
            var expense = await _unitOfWork.Expense.GetByIdAsync(id);
            if (expense == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Message = $"Expense with ID {id} not found.";
                respons.Success = false;
                respons.Data = null;
                // throw new KeyNotFoundException($"Expense with ID {id} not found.");
            }
            respons.StatusCode = (int)HttpStatusCode.NoContent;
            respons.Message = "Success";
            respons.Success = true;
            respons.Data = null;
            await _unitOfWork.Expense.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return respons ;
        }

        public async Task<GenericResponse<IEnumerable<ExpenseResponseDto>>> GetAllExpenseAsync()
        {
            var respons = new GenericResponse<IEnumerable<ExpenseResponseDto>>();
            var expenses = await _unitOfWork.Expense.GetAllAsync();
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Message = "Success";
            respons.Success = true;
            respons.Data = _mapper.Map<IEnumerable<ExpenseResponseDto>>(expenses);
            return respons;
        }

        public async Task<GenericResponse<ExpenseResponseDto>> GetExpenseByIdAsync(int id)
        {
            var respons= new GenericResponse<ExpenseResponseDto>();
            var expense = await _unitOfWork.Expense.GetByIdAsync(id);
            if (expense == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Message = $"Expense with ID {id} not found.";
                respons.Success = false;
                respons.Data = null;
                return respons;
                //throw new KeyNotFoundException($"Expense with ID {id} not found.");
            }
      
            respons.StatusCode = (int)HttpStatusCode.NoContent;
            respons.Message = "Success";
            respons.Success = true;
            respons.Data = _mapper.Map<ExpenseResponseDto>(expense);
            return respons;
        }

        public async Task<GenericResponse<IEnumerable<ExpenseResponseDto>>> GetExpensesByRepresentativeIdAsync(int representativeId)
        {
            var respons=new GenericResponse<IEnumerable<ExpenseResponseDto>>();
            var representative = await _unitOfWork.Representatives.GetByIdAsync(representativeId);
            if (representative == null)
            {
                respons.StatusCode = (int)HttpStatusCode.NotFound;
                respons.Message = $"Representative with ID {representativeId} not found.";
                respons.Success = false;
                respons.Data = null;
                return respons;
               // throw new KeyNotFoundException($"Representative with ID {representativeId} not found.");
            }
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Message = "Success";
            respons.Success = true;
            var expenses = await _unitOfWork.Expense.GetByRepresentativeIdAsync(representativeId);
            respons.Data = _mapper.Map<IEnumerable<ExpenseResponseDto>>(expenses);
            return respons;
        }

        public async Task<GenericResponse<ExpenseResponseDto>> UpdateExpenseAsync(int id,ExpenseCreateDto expenseDto)
        {
            var respons=new GenericResponse<ExpenseResponseDto>();
            if (expenseDto == null)
            {
                respons.StatusCode= (int)HttpStatusCode.BadRequest;
                respons.Success= false;
                respons.Message = "Expense  data is required.";
                //throw new ArgumentNullException(nameof(expenseDto));
                return respons;
            }

            var existingExpense = await _unitOfWork.Expense.GetByIdAsync(id);
            if (existingExpense == null)
            {
                respons.StatusCode=(int)HttpStatusCode.NotFound;
                respons.Success=false;
                respons.Message = $"Expense with ID {id} not found.";
                return respons;
                //throw new KeyNotFoundException($"Expense with ID {id} not found.");
            }
            

            if (expenseDto.RepresentativeId.HasValue)
            {
                var representative = await _unitOfWork.Representatives.GetByIdAsync(expenseDto.RepresentativeId.Value);
                if (representative == null)
                {
                    respons.StatusCode = (int)HttpStatusCode.NotFound;
                    respons.Success = false;
                    respons.Message = "Representative not found.";
                    return respons;
                    // throw new BusinessException("Representative not found.");
                }
            }
            var allowedCurrencies = new[] { "EGP", "$", "USD", "EUR" };
            expenseDto.Amount.Currency = expenseDto.Amount.Currency?.Trim();
            if (string.IsNullOrWhiteSpace(expenseDto.Amount.Currency) || !allowedCurrencies.Contains(expenseDto.Amount.Currency))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Invalid currency , Allowed values are: EGP , $ , USD , EUR ";
                respons.Data = null;
                return respons;
            }
            if (!expenseDto.ExpenseType.HasValue || !Enum.IsDefined(typeof(Domain.Enums.ExpenseType), expenseDto.ExpenseType.Value))
            {
                respons.StatusCode = (int)HttpStatusCode.BadRequest;
                respons.Success = false;
                respons.Message = "Invalid or missing expense type. Allowed values are: General, Advance, Fuel, Rent, Maintenance.";
                respons.Data = null;
                return respons;
            }
            _mapper.Map(expenseDto, existingExpense);
            existingExpense.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Expense.UpdateAsync(existingExpense);
            await _unitOfWork.SaveChangesAsync();
            respons.StatusCode = (int)HttpStatusCode.OK;
            respons.Success = true;
            respons.Message = "Success";
            respons.Data = _mapper.Map<ExpenseResponseDto>(existingExpense);
            return respons;
          
        }
        
    }
}

