using Microsoft.AspNetCore.Mvc;
using UserService.Dtos;
using UserService.Services;
using Microsoft.AspNetCore.Authorization;
using UserService.Models;
using System.Security.Claims;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        private ApiErrorResponse BuildError(string message)
        {
            return ApiErrorResponse.Create(message, HttpContext.TraceIdentifier);
        }

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(BuildError(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while registering user with email {Email}.", request.Email);
                return StatusCode(500, BuildError("An unexpected error occurred while registering the user."));
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(BuildError(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while logging in user with email {Email}.", request.Email);
                return StatusCode(500, BuildError("An unexpected error occurred while logging in."));
            }

        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            string? user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(user, out int userId))
            {
                return Unauthorized(BuildError("Invalid token subject"));
            }
            try
            {
                var result = await _authService.GetProfileAsync(userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(BuildError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving profile for user id {UserId}.", userId);
                return StatusCode(500, BuildError("An unexpected error occurred while fetching profile."));
            }
        }

        [HttpPatch("{id:int}")]
        [Authorize(Roles = nameof(UserRoles.Admin))]
        public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleRequest changeRoleRequest)
        {
            try
            {
                var result = await _authService.ChangeRoleAsync(id, changeRoleRequest.Role);
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
                _logger.LogError(ex, "Unexpected error while changing role for user id {UserId}.", id);
                return StatusCode(500, BuildError("An unexpected error occurred while changing role."));
            }
        }
    }
}