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
        private readonly ILogger<ProductsController> _logger;

        private ApiErrorResponse BuildError(string message)
        {
            return ApiErrorResponse.Create(message, HttpContext.TraceIdentifier);
        }

        public ProductsController(IProductCatalogService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponse>> CreateAsync([FromBody] CreateProductRequest request)
        {
            try
            {
                var result = await _productService.CreateAsync(request);
                return Created($"/api/products/{result.Id}", result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BuildError(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating product.");
                return StatusCode(500, BuildError("An unexpected error occurred while creating the product."));
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductResponse>> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest(BuildError($"Invalid ID: {id}. Product ID must be greater than zero."));
            }

            try
            {
                var result = await _productService.GetByIdAsync(id);

                if (result is null)
                {
                    return NotFound(BuildError($"Product with ID {id} was not found."));
                }

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting product by id {ProductId}.", id);
                return StatusCode(500, BuildError("An unexpected error occurred while processing your request."));
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllAsync(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _productService.GetAllAsync(search, categoryId, page, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting product list. Search={Search}, CategoryId={CategoryId}, Page={Page}, PageSize={PageSize}", search, categoryId, page, pageSize);
                return StatusCode(500, BuildError("An unexpected error occurred while processing your request."));
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
                return NotFound(BuildError(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BuildError(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating product {ProductId}.", id);
                return StatusCode(500, BuildError("An unexpected error occurred while updating the product."));
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
                return NotFound(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting product {ProductId}.", id);
                return StatusCode(500, BuildError("An unexpected error occurred while deleting the product."));
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
                return NotFound(BuildError(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating stock for product {ProductId}.", id);
                return StatusCode(500, BuildError("An unexpected error occurred while updating the stock."));
            }
        }
    }
}