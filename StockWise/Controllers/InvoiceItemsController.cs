using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Services.DTOS;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceItemsController : ControllerBase
    {

        private readonly IInvoiceItemService _invoiceItemService;

        public InvoiceItemsController(IInvoiceItemService invoiceItemService)
        {
            _invoiceItemService = invoiceItemService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var invoiceItems = await _invoiceItemService.GetAllInvoiceItemAsync();
                return Ok(invoiceItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var invoiceItem = await _invoiceItemService.GetInvoiceItemByIdAsync(id);
                return Ok(invoiceItem);
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
        public async Task<IActionResult> Create([FromBody] InvoiceItemDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _invoiceItemService.CreateInvoiceItemAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
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
        public async Task<IActionResult> Update(int id, [FromBody] InvoiceItemDto dto)
        {
            try
            {
                if (id != dto.Id)
                    return BadRequest("ID mismatch");

                await _invoiceItemService.UpdateInvoiceItemAsync(dto);
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
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _invoiceItemService.DeleteInvoiceItemAsync(id);
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
