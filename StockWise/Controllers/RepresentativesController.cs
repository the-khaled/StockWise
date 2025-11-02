using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.RepresentativeDto;
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
                if (!representative.Success)
                {
                    return StatusCode(representative.StatusCode, representative);
                }
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
        public async Task<IActionResult> Create([FromBody] RepresentativeCreateDto dto)
        {
            try
            {
                var createdRepresentative = await _representativeService.CreateRepresentativeAsync(dto);

                if (!createdRepresentative.Success) 
                {
                    return StatusCode(createdRepresentative.StatusCode, createdRepresentative);
                }
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    return BadRequest(new { errors });
                }

                return CreatedAtAction(nameof(GetById), new { id = createdRepresentative.Data.Id }, createdRepresentative);
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
        public async Task<IActionResult> Update(int id, [FromBody] RepresentativeCreateDto dto)
        {
            try
            {
                var updatedRepresentative = await _representativeService.UpdateRepresentativeAsync(id, dto);

                if (!updatedRepresentative.Success)
                {
                    return StatusCode(updatedRepresentative.StatusCode, updatedRepresentative);
                }
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();
                    return BadRequest(new { errors });
                }

                return Ok(updatedRepresentative);
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
        [HttpGet("by-warehouse/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouseId(int warehouseId)
        {
            try
            {
                var representatives = await _representativeService.GetRepresentativesByWarehouseIdAsync(warehouseId);
                if (!representatives.Success)
                {
                    return StatusCode(representatives.StatusCode, representatives);
                }
                return Ok(representatives);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpGet("by-nationalid")]
        public async Task<IActionResult> GetByNationalId([FromQuery] string nationalId)
        {
            try
            {
                var representative = await _representativeService.GetRepresentativeByNationalIdAsync(nationalId);
                if (!representative.Success)
                {
                    return StatusCode(representative.StatusCode, representative);
                }
                if (string.IsNullOrWhiteSpace(nationalId))
                    return BadRequest(new { error = "National ID cannot be empty or whitespace." });

                return Ok(representative);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var representative = await _representativeService.GetRepresentativeByIdAsync(id);
                if (!representative.Success)
                {
                    return StatusCode(representative.StatusCode, representative);
                }
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
