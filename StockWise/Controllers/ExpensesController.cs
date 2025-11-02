using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.CustomerDto;
using StockWise.Services.DTOS.ExpenseDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using StockWise.Services.ServicesResponse;
using System.Net;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpensesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var expenses = await _expenseService.GetAllExpenseAsync();
                return Ok(expenses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpGet("by-representative/{representativeId}")]
        public async Task<IActionResult> GetByRepresentativeId(int representativeId)
        {
            try
            {
                var expenses = await _expenseService.GetExpensesByRepresentativeIdAsync(representativeId);
                if (!expenses.Success)
                {
                    return StatusCode(expenses.StatusCode, expenses);
                }
                return Ok(expenses);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var expense = await _expenseService.GetExpenseByIdAsync(id);
                if (!expense.Success)
                {
                    return StatusCode(expense.StatusCode, expense);
                }
                return Ok(expense);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ExpenseCreateDto expenseDto)
        {
            try
            {
                var createdExpense = await _expenseService.CreateExpenseAsync(expenseDto);

                if (!ModelState.IsValid|| createdExpense.Data==null)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    return BadRequest(new { errors });
                }
                if (!createdExpense.Success)
                {
                    return StatusCode(createdExpense.StatusCode, createdExpense);
                }
                return CreatedAtAction(nameof(GetById), new { id = createdExpense.Data.Id }, createdExpense);
                
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ExpenseCreateDto expenseDto)
        {
            try
            {
                var updatedExpense = await _expenseService.UpdateExpenseAsync(id, expenseDto);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    return BadRequest(new { errors });
                }
                if (!updatedExpense.Success)
                {
                    return StatusCode(updatedExpense.StatusCode, updatedExpense);
                }

                return Ok(updatedExpense);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleteexpense= await _expenseService.DeleteExpenseAsync(id);
                if (!deleteexpense.Success)
                {
                    return StatusCode(deleteexpense.StatusCode, deleteexpense);
                }
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
