using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Services.DTOS.InvoiceDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using StockWise.Services.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var invoices = await _invoiceService.GetAllInvoicesAsync();
                return Ok(invoices);
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

                var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
                if (!invoice.Success)
                {
                    return StatusCode(invoice.StatusCode, invoice);
                }
                return Ok(invoice);
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
        public async Task<IActionResult> Create([FromBody] InvoiceCreateDto invoiceDto)
        {
            try
            {
                var createdInvoice = await _invoiceService.CreateInvoiceAsync(invoiceDto);

    /*            if (!ModelState.IsValid)
                {
                    var errors = ModelState
                       .SelectMany(x => x.Value.Errors)
                       .Select(x => x.ErrorMessage)
                       .ToList();
                    return BadRequest(new { errors });
                }*/

                if (!createdInvoice.Success)
                {
                    return StatusCode(createdInvoice.StatusCode, createdInvoice);
                }
                return CreatedAtAction(nameof(GetById), new { id = createdInvoice.Data.Id }, createdInvoice);
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
        public async Task<IActionResult> Update(int id, [FromBody] InvoiceCreateDto updateDto)
        {
            try
            {
               var UpdateInvois= await _invoiceService.UpdateInvoiceAsync(id, updateDto);

         /*       if (!ModelState.IsValid)
                {
                    var errors = ModelState
                       .SelectMany(x => x.Value.Errors)
                       .Select(x => x.ErrorMessage)
                       .ToList();
                    return BadRequest(new { errors });
                }*/
                if (!UpdateInvois.Success)
                {
                    return StatusCode(UpdateInvois.StatusCode, UpdateInvois);
                }
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
               var deletedExist= await _invoiceService.DeleteInvoiceAsync(id);
                if (!deletedExist.Success)
                {
                    return StatusCode(deletedExist.StatusCode, deletedExist);
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
