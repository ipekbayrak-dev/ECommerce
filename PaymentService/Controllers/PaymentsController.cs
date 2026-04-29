using Microsoft.AspNetCore.Mvc;
using PaymentService.Dtos;
using PaymentService.Services;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentManagementService _paymentManagementService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentManagementService paymentManagementService, ILogger<PaymentsController> logger)
        {
            _paymentManagementService = paymentManagementService;
            _logger = logger;
        }

        private ApiErrorResponse BuildError(string message) =>
            ApiErrorResponse.Create(message, HttpContext.TraceIdentifier);

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PaymentResponse>> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest(BuildError("ID must be a positive integer."));
            }

            try
            {
                var response = await _paymentManagementService.GetByIdAsync(id);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting payment {PaymentId}.", id);
                return StatusCode(500, BuildError("An unexpected error occurred."));
            }
        }
        [HttpGet("order/{orderId:int}")]
        public async Task<ActionResult<PaymentResponse>> GetByOrderIdAsync(int orderId)
        {
            if (orderId <= 0)
            {
                return BadRequest(BuildError("Order ID must be a positive integer."));
            }

            try
            {
                var response = await _paymentManagementService.GetByOrderIdAsync(orderId);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting payment for order {OrderId}.", orderId);
                return StatusCode(500, BuildError("An unexpected error occurred."));
            }
        }

        [HttpPost]
        public async Task<ActionResult<PaymentResponse>> CreatePayment(CreatePaymentRequest request)
        {
            try
            {
                var response = await _paymentManagementService.CreatePaymentAsync(request);
                return Created($"/api/payments/{response.Id}", response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System error while creating payment.");
                return StatusCode(500, BuildError("An unexpected error occurred."));
            }
        }
        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebHookAsync([FromHeader(Name = "Stripe-Signature")] string stripeSignature)
        {
            if (string.IsNullOrWhiteSpace(stripeSignature))
            {
                return BadRequest("Missing Stripe signature.");
            }

            try
            {
                using var streamReader = new StreamReader(HttpContext.Request.Body);
                var stripeEventJson = await streamReader.ReadToEndAsync();

                await _paymentManagementService.HandleWebHookAsync(stripeEventJson, stripeSignature);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Stripe Webhook validation failed.");
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing the Stripe Webhook.");
                return StatusCode(500);
            }
        }
    }
}