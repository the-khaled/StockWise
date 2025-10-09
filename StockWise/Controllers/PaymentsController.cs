using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() 
        {
            try 
            { 
                var payments= await _paymentService.GetAllPaymentAsync();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) 
        {
            try {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }//لازم دا الاول 

            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }//دا الي بعدو 
           
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentDto paymentDto) 
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                await _paymentService.CreatePaymentAsync(paymentDto);
                return CreatedAtAction(nameof(GetById), new { id = paymentDto.Id }, paymentDto);
            }
            catch (BusinessException ex){  return BadRequest(new { error = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentDto Paymentdto)
        {
            try
            {
                if (id != Paymentdto.Id)
                    return BadRequest("ID mismatch");

                await _paymentService.UpdatePaymentAsync(Paymentdto);
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
                await _paymentService.DeletePaymentAsync(id);
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
