using InventoryService.Consumers;
using InventoryService.Data;
using InventoryService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var inventoryDbConnectionString = builder.Configuration.GetConnectionString("InventoryDb");
if (string.IsNullOrWhiteSpace(inventoryDbConnectionString))
    throw new InvalidOperationException("Inventory DB connection string is missing. Set ConnectionStrings__InventoryDb environment variable.");

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(inventoryDbConnectionString));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer, OrderPlacedConsumerDefinition>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        cfg.Host(rabbitHost);
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<IInventoryManagementService, InventoryManagementService>();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
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

app.MapGet("/", () => Results.Content("""
<html><body style="font-family:sans-serif;padding:2rem;background:#0f0f0f;color:#fff">
<h2>InventoryService is running</h2>
<a href="/scalar/v1" style="color:#60a5fa">Open Scalar UI</a>
</body></html>
""", "text/html"));

app.MapControllers();
app.Run();
