using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductService.Data;
using ProductService.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var productDbConnectionString = builder.Configuration.GetConnectionString("ProductDb");
if (string.IsNullOrWhiteSpace(productDbConnectionString))
{
    throw new InvalidOperationException("Product DB connection string is missing. Set ConnectionStrings__ProductDb in user-secrets or environment variables.");
}
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("JWT secret is missing. Set Jwt__Secret environment variable.");
var redis = builder.Configuration["Redis:ConnectionString"];
if (string.IsNullOrWhiteSpace(redis))
    throw new InvalidOperationException("Application startup failed: 'Redis:ConnectionString' is not configured. Ensure it is set in appsettings.json or environment variables.");
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(productDbConnectionString));
builder.Services.AddScoped<IProductCatalogService, ProductCatalogService>();
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
builder.Services.AddAuthorization();
builder.Services.AddStackExchangeRedisCache(x => x.Configuration = redis);

var app = builder.Build();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => Results.Content("""
<html><body style="font-family:sans-serif;padding:2rem;background:#0f0f0f;color:#fff">
<h2>ProductService is running</h2>
<a href="/scalar/v1" style="color:#60a5fa">Open Scalar UI</a>
</body></html>
""", "text/html"));

app.MapControllers();
app.Run();