using UserService.Dtos;
using UserService.Models;

namespace UserService.Services
{
    public interface IAuthService
    {
        public Task<AuthResponse> RegisterAsync(RegisterRequest request);
        public Task<AuthResponse> LoginAsync(LoginRequest request);
        public Task<UserProfileResponse> GetProfileAsync(int userId);
        public Task<UserProfileResponse> ChangeRoleAsync(int userId, UserRoles role);
    }
}