using UserService.Models;
using Xunit;

namespace UserService.Tests;

public class UserModelTests
{
    [Fact]
    public void SetPassword_HashesPassword()
    {
        var user = new User { Username = "ipek", Email = "ipek@test.com" };
        user.SetPassword("Secret123!");

        Assert.NotNull(user.PasswordHash);
        Assert.NotEqual("Secret123!", user.PasswordHash);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var user = new User { Username = "ipek", Email = "ipek@test.com" };
        user.SetPassword("Secret123!");

        Assert.True(user.VerifyPassword("Secret123!"));
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var user = new User { Username = "ipek", Email = "ipek@test.com" };
        user.SetPassword("Secret123!");

        Assert.False(user.VerifyPassword("Wrong!"));
    }

    [Fact]
    public void NewUser_DefaultRole_IsCustomer()
    {
        var user = new User { Username = "ipek", Email = "ipek@test.com" };

        Assert.Equal(UserRoles.Customer, user.Role);
    }
}
