using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using UserService.Data;
using UserService.Dtos;
using UserService.Models;
using UserService.Services;
using Xunit;

namespace UserService.Tests;

public class AuthServiceTests
{
    private static UserDbContext CreateDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new UserDbContext(options);
    }

    private static IConfiguration BuildConfig(double expiryHours = 1)
    {
        var dict = new Dictionary<string, string?> { ["Jwt:ExpiryHours"] = expiryHours.ToString() };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    private static AuthService BuildService(UserDbContext db, ITokenService? tokenService = null)
    {
        var token = tokenService ?? Mock.Of<ITokenService>(t =>
            t.GenerateToken(It.IsAny<User>()) == "fake-token");
        return new AuthService(db, token, BuildConfig());
    }

    // ── RegisterAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsAuthResponse()
    {
        using var db = CreateDb(nameof(RegisterAsync_ValidRequest_ReturnsAuthResponse));
        var svc = BuildService(db);

        var result = await svc.RegisterAsync(new RegisterRequest
        {
            Email = "ipek@test.com",
            Username = "ipek",
            Password = "Secret123!"
        });

        Assert.Equal("ipek", result.Username);
        Assert.Equal("fake-token", result.Token);
        Assert.Single(await db.Users.ToListAsync());
    }

    [Fact]
    public async Task RegisterAsync_NormalizesEmailToLowercase()
    {
        using var db = CreateDb(nameof(RegisterAsync_NormalizesEmailToLowercase));
        var svc = BuildService(db);

        await svc.RegisterAsync(new RegisterRequest
        {
            Email = "IPEK@TEST.COM",
            Username = "ipek",
            Password = "Secret123!"
        });

        var user = await db.Users.SingleAsync();
        Assert.Equal("ipek@test.com", user.Email);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        using var db = CreateDb(nameof(RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException));
        var svc = BuildService(db);

        await svc.RegisterAsync(new RegisterRequest { Email = "dupe@test.com", Username = "a", Password = "P@ss1" });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.RegisterAsync(new RegisterRequest { Email = "dupe@test.com", Username = "b", Password = "P@ss2" }));
    }

    [Theory]
    [InlineData("", "user", "pass")]
    [InlineData("e@e.com", "", "pass")]
    [InlineData("e@e.com", "user", "")]
    public async Task RegisterAsync_MissingField_ThrowsArgumentException(string email, string username, string password)
    {
        using var db = CreateDb($"reg-missing-{email}{username}{password}");
        var svc = BuildService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.RegisterAsync(new RegisterRequest { Email = email, Username = username, Password = password }));
    }

    // ── LoginAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        using var db = CreateDb(nameof(LoginAsync_ValidCredentials_ReturnsAuthResponse));
        var svc = BuildService(db);

        await svc.RegisterAsync(new RegisterRequest { Email = "login@test.com", Username = "user", Password = "P@ss1!" });

        var result = await svc.LoginAsync(new LoginRequest { Email = "login@test.com", Password = "P@ss1!" });

        Assert.Equal("user", result.Username);
        Assert.Equal("fake-token", result.Token);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        using var db = CreateDb(nameof(LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException));
        var svc = BuildService(db);

        await svc.RegisterAsync(new RegisterRequest { Email = "u@t.com", Username = "u", Password = "CorrectP@ss1" });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.LoginAsync(new LoginRequest { Email = "u@t.com", Password = "WrongPass" }));
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsUnauthorizedAccessException()
    {
        using var db = CreateDb(nameof(LoginAsync_UnknownEmail_ThrowsUnauthorizedAccessException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.LoginAsync(new LoginRequest { Email = "nobody@test.com", Password = "X" }));
    }

    [Theory]
    [InlineData("", "pass")]
    [InlineData("e@e.com", "")]
    public async Task LoginAsync_MissingField_ThrowsArgumentException(string email, string password)
    {
        using var db = CreateDb($"login-missing-{email}{password}");
        var svc = BuildService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.LoginAsync(new LoginRequest { Email = email, Password = password }));
    }

    // ── GetProfileAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetProfileAsync_ExistingUser_ReturnsProfile()
    {
        using var db = CreateDb(nameof(GetProfileAsync_ExistingUser_ReturnsProfile));
        var svc = BuildService(db);

        var reg = await svc.RegisterAsync(new RegisterRequest { Email = "p@t.com", Username = "profuser", Password = "P@ss1!" });

        var profile = await svc.GetProfileAsync(reg.UserId);

        Assert.Equal("profuser", profile.Username);
        Assert.Equal("p@t.com", profile.Email);
    }

    [Fact]
    public async Task GetProfileAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb(nameof(GetProfileAsync_NotFound_ThrowsKeyNotFoundException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.GetProfileAsync(9999));
    }

    // ── ChangeRoleAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ChangeRoleAsync_ValidUser_UpdatesRole()
    {
        using var db = CreateDb(nameof(ChangeRoleAsync_ValidUser_UpdatesRole));
        var svc = BuildService(db);

        var reg = await svc.RegisterAsync(new RegisterRequest { Email = "role@t.com", Username = "r", Password = "P@ss1!" });

        var result = await svc.ChangeRoleAsync(reg.UserId, UserRoles.Admin);

        Assert.Equal(UserRoles.Admin, result.Role);
    }

    [Fact]
    public async Task ChangeRoleAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb(nameof(ChangeRoleAsync_NotFound_ThrowsKeyNotFoundException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.ChangeRoleAsync(999, UserRoles.Admin));
    }
}
