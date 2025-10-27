using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.TransferDto;
using StockWise.Services.IServices;
using StockWise.Services.Services;

namespace StockWise.Controllers
{
    [Authorize]   
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

        [HttpGet("warehouse/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouseId(int warehouseId)
        {
            try
            {
                var transfers = await _transfer.GetByWarehouseIdAsync(warehouseId);
                return Ok(transfers);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransferCreateDto Transferdto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var createdTransfer = await _transfer.CreateTransferAsync(Transferdto);
                return CreatedAtAction(nameof(GetById), new { id = createdTransfer.Id }, createdTransfer);
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
        public async Task<IActionResult> Update(int id, [FromBody] TransferCreateDto transferdto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedTransfer = await _transfer.UpdateTransferAsync(id, transferdto);
                return Ok(updatedTransfer);
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
        public async Task<IActionResult> cancel(int id)
        {
            try
            {
                await _transfer.cancelTransferAsync(id);
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
