using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransfersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public TransfersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transfers = await _unitOfWork.Transfers.GetAllAsync();
            return Ok(transfers);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transfers = await _unitOfWork.Transfers.GetByIdAsync(id);
            if (transfers == null) return NotFound();
            return Ok(transfers);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Transfer Transfer)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _unitOfWork.Transfers.AddAsync(Transfer);
            await _unitOfWork.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = Transfer.Id }, Transfer);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Transfer transfer)
        {
            if (id != transfer.Id) return BadRequest("ID not match");
            var Transfer = await _unitOfWork.Transfers.GetByIdAsync(id);

            if (transfer == null) return NotFound();
            await _unitOfWork.Transfers.UpdateAsync(transfer);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transfer = await _unitOfWork.Transfers.GetByIdAsync(id);
            if (transfer == null)
                return NotFound();

            await _unitOfWork.Transfers.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }
    }
}
