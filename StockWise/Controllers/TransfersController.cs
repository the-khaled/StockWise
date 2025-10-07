using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.IServices;
using StockWise.Services.Services;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransfersController : ControllerBase
    {
        private readonly ITransferService _transfer;
        public TransfersController(ITransferService transfer)
        {
            _transfer = transfer;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var transfers = await _transfer.GetAllTransfersAsync();
                return Ok(transfers);
            } catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }
            [HttpGet("{id}")]
            public async Task<IActionResult> GetById(int id)
            {
                try
                {
                var transfer = await _transfer.GetTransferByIdAsync(id);
                return Ok(transfer);
               
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
        public async Task<IActionResult> Create([FromBody] TransferDto Transfer)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                await _transfer.CreateTransferAsync(Transfer);
                return CreatedAtAction(nameof(GetById), new { id = Transfer.Id }, Transfer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
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
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TransferDto transfer)
        {
            try
            {
                if (id != transfer.Id)
                    return BadRequest("ID mismatch");

                await _transfer.UpdateTransferAsync(transfer);
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _transfer.DeleteTransferAsync(id);
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
