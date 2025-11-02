using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.WarehouseDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Threading.Tasks;

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
                var warehouses = await _warehouseService.GetAllWarehousesAsync();
                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var warehouse = await _warehouseService.GetWarehouseByIdAsync(id);
                if (!warehouse.Success)
                    return StatusCode(warehouse.StatusCode, warehouse);
                return Ok(warehouse);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
        [HttpGet("search")]
        public async Task<IActionResult> GetWarehousesByName([FromQuery] string name)
        {
            try
            {
                var warehouses = await _warehouseService.GetWarehousesByNameAsync(name);
                if (!warehouses.Success)
                    return StatusCode(warehouses.StatusCode, warehouses);
                return Ok(warehouses);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WarehouseCreateDto warehouseDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdWarehouse = await _warehouseService.CreateWarehouseAsync(warehouseDto);
                if (!createdWarehouse.Success)
                    return StatusCode(createdWarehouse.StatusCode, createdWarehouse);
                return CreatedAtAction(nameof(GetById), new { id = createdWarehouse.Data.Id }, createdWarehouse);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WarehouseCreateDto warehouseDto)
        {
            try
            {
                var updatedWarehouse = await _warehouseService.UpdateWarehouseAsync(id, warehouseDto);

                if (!updatedWarehouse.Success)
                    return StatusCode(updatedWarehouse.StatusCode, updatedWarehouse);
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                return Ok(updatedWarehouse);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var warhous = await _warehouseService.GetWarehouseByIdAsync(id);
                if (!warhous.Success)
                    return StatusCode(warhous.StatusCode, warhous);
                await _warehouseService.DeleteWarehouseAsync(id);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

     
    }
}