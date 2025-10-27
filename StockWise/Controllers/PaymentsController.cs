using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.PaymentDto;
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
                var payments = await _paymentService.GetAllPaymentAsync();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);

                if (!payment.Success)
                {
                    return StatusCode(payment.StatusCode, payment);
                }
                return Ok(payment);
            }
            catch (BusinessException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("invoice/{invoiceId}")]
        public async Task<IActionResult> GetByInvoiceId(int invoiceId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByInvoiceIdAsync(invoiceId);
                if (!payments.Success)
                {
                    return StatusCode(payments.StatusCode, payments);
                }
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomerId(int customerId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByCustomerIdAsync(customerId);

                if (!payments.Success)
                {
                    return StatusCode(payments.StatusCode, payments);
                }
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingPayments()
        {
            try
            {
                var payments = await _paymentService.GetPendingPaymentsAsync();
                if (!payments.Success)
                {
                    return StatusCode(payments.StatusCode, payments);
                }
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentCreateDto paymentDto)
        {
            try
            {
                var createdPayment = await _paymentService.CreatePaymentAsync(paymentDto);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!createdPayment.Success)
                {
                    return StatusCode(createdPayment.StatusCode, createdPayment);
                }
                return CreatedAtAction(nameof(GetById), new { id = createdPayment.Data.Id }, createdPayment);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentCreateDto paymentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedPayment = await _paymentService.UpdatePaymentAsync(id, paymentDto);
                if (!updatedPayment.Success)
                {
                    return StatusCode(updatedPayment.StatusCode, updatedPayment);
                }
                return Ok(updatedPayment);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _paymentService.CancelPaymentAsync(id);
                return NoContent();
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

       

       
    }
}
