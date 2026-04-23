using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Services;

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
}

app.MapControllers();
app.Run();