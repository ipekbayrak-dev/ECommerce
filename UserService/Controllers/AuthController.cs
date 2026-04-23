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
        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            string? user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(user, out int userId))
            {
                return Unauthorized("Invalid token subject");
            }
            try
            {
                var result = await _authService.GetProfileAsync(userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
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
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}