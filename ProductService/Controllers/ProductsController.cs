using Microsoft.AspNetCore.Mvc;
using ProductService.Dtos;
using ProductService.Services;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductCatalogService _productService;

        public ProductsController(IProductCatalogService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponse>> CreateAsync([FromBody] CreateProductRequest request)
        {
            try
            {
                var result = await _productService.CreateAsync(request);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred while creating the product." });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductResponse>> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = $"Invalid ID: {id}. Product ID must be greater than zero." });
            }

            try
            {
                var result = await _productService.GetByIdAsync(id);

                if (result is null)
                {
                    return NotFound(new { Message = $"Product with ID {id} was not found." });
                }

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred while processing your request." });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllAsync([FromQuery] string? search, [FromQuery] int? categoryId)
        {
            try
            {
                var result = await _productService.GetAllAsync(search, categoryId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred while processing your request." });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductResponse>> UpdateAsync(int id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                var result = await _productService.UpdateAsync(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred while updating the product." });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            try
            {
                await _productService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred while deleting the product." });
            }
        }

        [HttpPatch("{id:int}/stock")]
        public async Task<ActionResult> UpdateStockAsync(int id, [FromBody] UpdateStockRequest request)
        {
            try
            {
                var result = await _productService.UpdateStockAsync(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred while updating the stock." });
            }
        }
    }
}