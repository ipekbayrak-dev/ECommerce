using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var productDbConnectionString = builder.Configuration.GetConnectionString("ProductDb");
if (string.IsNullOrWhiteSpace(productDbConnectionString))
{
    throw new InvalidOperationException("Product DB connection string is missing. Set ConnectionStrings__ProductDb in user-secrets or environment variables.");
}

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(productDbConnectionString));
builder.Services.AddScoped<IProductCatalogService, ProductCatalogService>();

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