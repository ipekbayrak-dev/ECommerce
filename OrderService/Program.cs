using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);
var orderDbConnectionString = builder.Configuration.GetConnectionString("OrderDb");
if (string.IsNullOrWhiteSpace(orderDbConnectionString))
{
    throw new InvalidOperationException("Order DB connection string is missing. Set ConnectionStrings__OrderDb in user-secrets or environment variables.");
}
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(orderDbConnectionString));
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        cfg.Host(rabbitHost);
    });
});
builder.Services.AddScoped<IOrderManagementService, OrderManagementService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
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

app.MapControllers();
app.Run();