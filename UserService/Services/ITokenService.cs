using UserService.Models;

namespace UserService.Services
{
    public interface ITokenService
    {
        public string GenerateToken(User user);
    }
}