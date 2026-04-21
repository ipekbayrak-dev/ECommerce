using UserService.Models;

namespace UserService.Dtos
{
    public class AuthResponse
    {
        public string? Token { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public int UserId { get; set; }
        public required string Username { get; set; }
        public UserRoles Role { get; set; }
    }
}