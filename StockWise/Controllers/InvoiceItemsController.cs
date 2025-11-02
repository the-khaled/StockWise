using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Services.DTOS.CustomerDto;
using StockWise.Services.DTOS.InvoiceDto;
using StockWise.Services.DTOS.InvoiceItemDto;
using StockWise.Services.DTOS.InvoiceItemDto.InvoiceItemDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using StockWise.Services.ServicesResponse;
using System.Net;

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
                if (!invoiceItem.Success)
                {
                    return StatusCode(invoiceItem.StatusCode, invoiceItem);
                }
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
        public async Task<IActionResult> Create([FromBody] InvoiceItemCreateDto createDto)
        {
            try
            {
                var createdItem = await _invoiceItemService.CreateInvoiceItemAsync(createDto);
                /*      if (!ModelState.IsValid|| createdItem.Data == null)
                      {


                          var response = new GenericResponse<CustomerResponseDto>
                          {
                              StatusCode = (int)HttpStatusCode.BadRequest,
                              Success = false,
                              Message = "Validation errors occurred.",
                              Data = null
                          };
                          return StatusCode(response.StatusCode, response);
                      }*/
                if (!createdItem.Success)
                {
                    return StatusCode(createdItem.StatusCode, createdItem);
                }

                return CreatedAtAction(nameof(GetById), new { id = createdItem.Data.Id }, createdItem);
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
        public async Task<IActionResult> Update(int id, [FromBody] InvoiceItemCreateDto updateDto)
        {
            try
            {
                var updatedItem = await _invoiceItemService.UpdateInvoiceItemAsync(id, updateDto);
                if (!updatedItem.Success)
                {
                    return StatusCode(updatedItem.StatusCode, updatedItem);
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
                var deletedInvoiceitem= await _invoiceItemService.DeleteInvoiceItemAsync(id);
                if (!deletedInvoiceitem.Success)
                    return StatusCode(deletedInvoiceitem.StatusCode,deletedInvoiceitem);

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
