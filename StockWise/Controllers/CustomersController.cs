using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.CustomerDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using StockWise.Services.ServicesResponse;
using System.Net;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return Ok(customers);
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
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            try
            {
                var customers = await _customerService.GetCustomersByNameAsync(name);

                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest(new { error = "Name cannot be empty or whitespace." });
                if (!customers.Success)
                {
                    return StatusCode(customers.StatusCode, customers);
                }
                return Ok(customers);
            }
            catch (ArgumentException ex)
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {

                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (!customer.Success)
                {
                    return StatusCode(customer.StatusCode, customer);
                }
                return Ok(customer);
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
        public async Task<IActionResult> Create([FromBody] CustomerCreateDto customerDto)
        {
            try
            {
                var createdCustomer = await _customerService.CreatCastomerAsync(customerDto);

                if (!createdCustomer.Success)
                {
                    return StatusCode(createdCustomer.StatusCode, createdCustomer);
                }
                if (!ModelState.IsValid|| createdCustomer.Data==null)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    return BadRequest(new { errors });
                }

                //return CreatedAtAction(nameof(GetById), new { id = createdCustomer.Data.Id }, createdCustomer.Data);
                return StatusCode(createdCustomer.StatusCode, createdCustomer);
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
        public async Task<IActionResult> Update(int id, [FromBody] CustomerCreateDto customerDto)
        {
            try
            {
                var updatedCustomer = await _customerService.UpdateCustomerAsync(id, customerDto);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    return BadRequest(new { errors });
                }
                if (!updatedCustomer.Success)
                {
                    return StatusCode(updatedCustomer.StatusCode, updatedCustomer);
                }

                return Ok(updatedCustomer);
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
               var deletedState= await _customerService.DeleteCustomerAsync(id);
                if (!deletedState.Success)
                {
                    return StatusCode(deletedState.StatusCode, deletedState);
                }
                return Ok(deletedState);
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
