using Microsoft.AspNetCore.Mvc;
using OrderService.Services;
using OrderService.Dtos;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderManagementService _orderManagementService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderManagementService orderManagementService, ILogger<OrdersController> logger)
        {
            _orderManagementService = orderManagementService;
            _logger = logger;
        }

        private ApiErrorResponse BuildError(string message) =>
            ApiErrorResponse.Create(message, HttpContext.TraceIdentifier);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAllAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest(BuildError("Page and pageSize must be greater than zero."));

            try
            {
                var result = await _orderManagementService.GetAllAsync();
                var paged = result.Skip((page - 1) * pageSize).Take(pageSize);
                return Ok(paged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting orders.");
                return StatusCode(500, BuildError("An unexpected error occurred."));
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderResponse>> GetByIdAsync(int id)
        {
            try
            {
                var result = await _orderManagementService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting order {OrderId}.", id);
                return StatusCode(500, BuildError("An unexpected error occurred."));
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetByUserIdAsync(int userId)
        {
            try
            {
                var orders = await _orderManagementService.GetByUserIdAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for user {UserId}.", userId);
                return StatusCode(500, BuildError("An unexpected error occurred."));
            }
        }

        [HttpPost]
        public async Task<ActionResult<OrderResponse>> CreateAsync([FromBody] CreateOrderRequest request)
        {
            try
            {
                var result = await _orderManagementService.CreateAsync(request);
                return Created($"/api/orders/{result.Id}", result);
            }
            catch (ArgumentException ex) 
            {
                return BadRequest(BuildError(ex.Message));
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "System error while creating order.");
                return StatusCode(500, BuildError("An unexpected error occurred."));
            }
        }

        [HttpPut("{id:int}/status")]
        public async Task<ActionResult<OrderResponse>> UpdateStatusAsync(int id, [FromBody] UpdateOrderRequest request)
        {
            try
            {
                var result = await _orderManagementService.UpdateStatusAsync(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(BuildError(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order {OrderId}.", id);
                return StatusCode(500, BuildError("An unexpected error occurred."));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<OrderResponse>> CancelAsync(int id)
        {
            try
            {
                var result = await _orderManagementService.CancelAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(BuildError(ex.Message));
            }
            catch (InvalidOperationException ex) 
            {
                return BadRequest(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling order {OrderId}.", id);
                return StatusCode(500, BuildError("An unexpected error occurred."));
            }
        }
    }
}