using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.StockDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StocksController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var stocks = await _stockService.GetAllStocksAsync();
                return Ok(stocks);
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
                var stock = await _stockService.GetStockByIdAsync(id);
                return Ok(stock);
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StockCreateDto stockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    return BadRequest(new { errors });
                }

                var createdStock = await _stockService.CreateStockAsync(stockDto);
                return CreatedAtAction(nameof(GetById), new { id = createdStock.Id }, createdStock);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { error = ex.Message });
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StockCreateDto stockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    return BadRequest(new { errors });
                }

                var updatedStock = await _stockService.UpdateStockAsync(id, stockDto);
                return Ok(updatedStock);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { error = ex.Message });
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
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _stockService.DeleteStockAsync(id);
                return NoContent();
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
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetByWarehouseAndProduct([FromQuery] int warehouseId, [FromQuery] int productId)
        {
            try
            {
                var stock = await _stockService.GetByWarehouseAndProductAsync(warehouseId, productId);
                return Ok(stock);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}