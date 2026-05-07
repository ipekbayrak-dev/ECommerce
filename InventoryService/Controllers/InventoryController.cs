using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InventoryService.Dtos;
using InventoryService.Services;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryManagementService _inventoryManagementService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            IInventoryManagementService inventoryManagementService,
            ILogger<InventoryController> logger)
        {
            _inventoryManagementService = inventoryManagementService;
            _logger = logger;
        }

        private ApiErrorResponse BuildError(string message) =>
            ApiErrorResponse.Create(message, HttpContext.TraceIdentifier);

        [HttpGet("{productId:int}")]
        public async Task<ActionResult<InventoryItemResponse>> GetByProductIdAsync(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest(BuildError("Product ID must be greater than zero."));
            }

            try
            {
                var response = await _inventoryManagementService.GetByProductIdAsync(productId);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Inventory search failed. ProductId {ProductId} not found.", productId);
                return NotFound(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical failure retrieving ProductId {ProductId}.", productId);
                return StatusCode(500, BuildError("An unexpected error occurred in the data warehouse."));
            }
        }

        [HttpPatch("{productId:int}/adjust")]
        public async Task<ActionResult<InventoryItemResponse>> AdjustStockAsync(int productId, [FromBody] AdjustStockRequest request)
        {
            if (productId <= 0)
            {
                return BadRequest(BuildError("Product ID must be greater than zero."));
            }

            try
            {
                var response = await _inventoryManagementService.AdjustStockAsync(productId, request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Adjustment failed. ProductId {ProductId} not found.", productId);
                return NotFound(BuildError(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business logic violation for ProductId {ProductId}. Negative stock attempted.", productId);
                return BadRequest(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical failure adjusting stock for ProductId {ProductId}.", productId);
                return StatusCode(500, BuildError("System overload while adjusting inventory."));
            }
        }

        [HttpPost("{productId:int}/seed")]
        public async Task<ActionResult<InventoryItemResponse>> SeedAsync(int productId, [FromBody] SeedInventoryRequest request)
        {
            if (productId <= 0)
            {
                return BadRequest(BuildError("Product ID must be greater than zero."));
            }

            try
            {
                var response = await _inventoryManagementService.SeedAsync(productId, request);
                return CreatedAtAction(nameof(GetByProductIdAsync), new { productId = response.ProductId }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Seed conflict for ProductId {ProductId}. Already initialized.", productId);
                return Conflict(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical failure seeding ProductId {ProductId}.", productId);
                return StatusCode(500, BuildError("Failed to plant the inventory seed."));
            }
        }
    }
}