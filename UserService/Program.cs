using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using UserService.Data;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("JWT secret is missing. Set Jwt__Secret environment variable.");
}

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddOpenApi();
var userDbConnectionString = builder.Configuration.GetConnectionString("UserDb");
if (string.IsNullOrWhiteSpace(userDbConnectionString))
    throw new InvalidOperationException("User DB connection string is missing. Set ConnectionStrings__UserDb environment variable.");
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(userDbConnectionString));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.Use(async (context, next) =>
{
    var incomingCorrelationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
    var correlationId = string.IsNullOrWhiteSpace(incomingCorrelationId)
        ? Guid.NewGuid().ToString("N")
        : incomingCorrelationId;

    context.TraceIdentifier = correlationId;
    context.Response.Headers["X-Correlation-ID"] = correlationId;

    using (app.Logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = correlationId
    }))
    {
        await next();
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Content("""
<html><body style="font-family:sans-serif;padding:2rem;background:#0f0f0f;color:#fff">
<h2>UserService is running</h2>
<a href="/scalar/v1" style="color:#60a5fa">Open Scalar UI</a>
</body></html>
""", "text/html"));

app.MapControllers();
app.Run();