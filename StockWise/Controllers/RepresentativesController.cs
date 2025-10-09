using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Services.DTOS;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepresentativesController : ControllerBase
    {
        private readonly IRepresentativeService _representativeService;
        public RepresentativesController(IRepresentativeService representativeService)
        {
            _representativeService = representativeService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var representatives = await _representativeService.GetAllRepresentativeAsync();
                return Ok(representatives);
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
                var representative = await _representativeService.GetRepresentativeByIdAsync(id);
                return Ok(representative);
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
        public async Task<IActionResult> Create([FromBody] RepresentativeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _representativeService.CreateRepresentativeAsync(dto);
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
        public async Task<IActionResult> Update(int id, [FromBody] RepresentativeDto dto)
        {
            try
            {
                if (id != dto.Id)
                    return BadRequest("ID mismatch");

                await _representativeService.UpdateRepresentativeAsync(dto);
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
                await _representativeService.DeleteRepresentativeAsync(id);
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
