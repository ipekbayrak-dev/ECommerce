using BCrypt.Net;

namespace UserService.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public string? PasswordHash { get; private set; }
        public UserRoles Role { get; set; } = UserRoles.Customer;

        public void SetPassWord(string plainPassword)
        {
            this.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
        }
        public bool VerifyPassword(string pass)
        {
            return BCrypt.Net.BCrypt.Verify(pass,PasswordHash);
        }
        
    }
}