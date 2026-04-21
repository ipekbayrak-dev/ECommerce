using UserService.Data;
using UserService.Dtos;
using UserService.Models;
using Microsoft.EntityFrameworkCore;

namespace UserService.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserDbContext _userDbContext;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        public AuthService(UserDbContext userDbContext, ITokenService tokenService, IConfiguration configuration)
        {
            _userDbContext = userDbContext;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
             string.IsNullOrWhiteSpace(request.Username) ||
             string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("All fields are required");
            }

            var normalizedUsername = request.Username.Trim();
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var userExists = await _userDbContext.Users.AnyAsync(u => u.Email == normalizedEmail);

            if (userExists)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            User user = new User { Username = normalizedUsername, Email = normalizedEmail };
            user.SetPassWord(request.Password);

            _userDbContext.Add(user);
            await _userDbContext.SaveChangesAsync();

            return new AuthResponse
            {
                Username = user.Username,
                Role = user.Role,
                UserId = user.Id,
                Token = _tokenService.GenerateToken(user),
                ExpiresAtUtc = DateTime.UtcNow.AddHours(_configuration.GetValue<double>("Jwt:ExpiryHours"))
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Email and Password are required.");
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = await _userDbContext.Users
                .SingleOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null || !user.VerifyPassword(request.Password))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role,
                Token = _tokenService.GenerateToken(user),
                ExpiresAtUtc = DateTime.UtcNow.AddHours(_configuration.GetValue<double>("Jwt:ExpiryHours"))
            };
        }

        public async Task<UserProfileResponse> GetProfileAsync(int userId)
        {
            var user = await _userDbContext.Users.SingleOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            return new UserProfileResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }
        public async Task<UserProfileResponse> ChangeRoleAsync(int userId, UserRoles role)
        {
            if (role != UserRoles.Admin && role != UserRoles.Customer)
            {
                throw new InvalidOperationException("Only Admin and Customer roles are allowed.");
            }

            var user = await _userDbContext.Users.SingleOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            user.Role = role;
            await _userDbContext.SaveChangesAsync();
            return new UserProfileResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}