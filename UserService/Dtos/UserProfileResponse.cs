using UserService.Models;

namespace UserService.Dtos
{
    public class UserProfileResponse
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public UserRoles Role { get; set; }
    }
}