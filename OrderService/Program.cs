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
builder.Services.AddScoped<IOrderManagementService, OrderManagementService>();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();