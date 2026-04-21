using UserService.Models;

namespace UserService.Dtos
{
    public class ChangeRoleRequest
    {
        public UserRoles Role { get; set; }
    }
}