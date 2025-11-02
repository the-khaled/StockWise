using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.DTOS.ProductDto;
using StockWise.Services.Exceptions;
using StockWise.Services.IServices;
using System;
using System.Threading.Tasks;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly  IWebHostEnvironment _env;
        public ProductsController(IProductService productService, IWebHostEnvironment env)
        {
            _productService = productService;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                if (!products.Success)
                    return StatusCode(products.StatusCode, products);
                return Ok(products.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.ToString() });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                if (!product.Success) 
                {
                    return StatusCode(product.StatusCode,product);
                }
                return Ok(product);
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
        public async Task<IActionResult> GetProductsByName([FromQuery] string name)
        {
            try
            {
                var products = await _productService.GetProductsByNameAsync(name);

                if (!products.Success)
                {
                    return StatusCode(products.StatusCode, products);
                }
                return Ok(products);
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
        [HttpGet("expiring/{daysBeforeExpiry}")]
        public async Task<IActionResult> GetExpiringProducts(int daysBeforeExpiry)
        {
            try
            {
                var products = await _productService.GetExpiringProductsAsync(daysBeforeExpiry);
                if (!products.Success)
                {
                    return StatusCode(products.StatusCode, products);
                }
                return Ok(products);
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

        [HttpGet("warehouse/{warehouseId}")]
        public async Task<IActionResult> GetProductsByWarehouse(int warehouseId)
        {
            try
            {
                var products = await _productService.GetProductsByWarehouseAsync(warehouseId);
                if (!products.Success)
                {
                    return StatusCode(products.StatusCode, products);
                }
                return Ok(products);
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
        public async Task<IActionResult> Create([FromForm] StandaloneProductCreateDto productDto)
        {
            try
            {

                var createdProduct = await _productService.CreateProductAsync(productDto,_env);
                if (!createdProduct.Success)
                    return StatusCode(createdProduct.StatusCode,createdProduct);
                //  إرجاعCreatedAtAction مع الـ ID الجديد
                if (createdProduct.Success && createdProduct.Data?.ImageUrl != null)
                {
                    createdProduct.Data.ImageUrl = Url.Content(createdProduct.Data.ImageUrl);
                }

                return CreatedAtAction(nameof(GetById), new { id = createdProduct.Data.Id }, createdProduct);
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
        public async Task<IActionResult> Update(int id, [FromBody] StandaloneProductCreateDto productDto, IWebHostEnvironment env)
        {
            try
            {

                var updatedProduct = await _productService.UpdateProductAsync(id, productDto ,env);
                if (!updatedProduct.Success)
                    return StatusCode(updatedProduct.StatusCode, updatedProduct);

                if (updatedProduct.Success && updatedProduct.Data?.ImageUrl != null)
                {
                    updatedProduct.Data.ImageUrl = Url.Content(updatedProduct.Data.ImageUrl);
                }

                return Ok(updatedProduct);
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
                var product = await _productService.GetProductByIdAsync(id);
                if (!product.Success)
                    return StatusCode(product.StatusCode, product);
                await _productService.DeleteProductAsync(id);
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