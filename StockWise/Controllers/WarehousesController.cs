using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Services.DTOS;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll() 
        {
            try
            {
                var warehouse = await _warehouseService.GetAllWarehousesAsync();
                return Ok(warehouse);
            }catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) 
        {
            try 
            { 
                var warehouse = await _warehouseService.GetWarehouseByIdAsync(id);
                return Ok(warehouse);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message }); }
        }
        [HttpPost]
        public async Task<IActionResult> Creat([FromBody] WarehouseDto warehouseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await _warehouseService.CreateWarehouseAsync(warehouseDto);
            return CreatedAtAction(nameof(GetById), new { id = warehouseDto.Id }, warehouseDto);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WarehouseDto dto)
        {
            try
            {
                if (id != dto.Id)
                    return BadRequest("ID mismatch");

                await _warehouseService.UpdateWarehouseAsync(dto);
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
                await _warehouseService.DeleteWarehouseAsync(id);
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
