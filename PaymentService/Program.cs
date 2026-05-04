using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Consumers;
using PaymentService.Data;
using PaymentService.Services;
using Stripe;
var builder = WebApplication.CreateBuilder(args);

var paymentDbConnectionString = builder.Configuration.GetConnectionString("PaymentDb");
var stripeSecret = builder.Configuration["Stripe:SecretKey"];
var stripeCurrency = builder.Configuration["Stripe:Currency"];
var stripeWebhook = builder.Configuration["Stripe:WebhookSecret"];

if (string.IsNullOrWhiteSpace(paymentDbConnectionString))
    throw new InvalidOperationException("Payment DB connection string is missing.");
if (string.IsNullOrWhiteSpace(stripeSecret))
    throw new InvalidOperationException("Stripe secret is missing.");
if (string.IsNullOrWhiteSpace(stripeCurrency))
    throw new InvalidOperationException("Stripe currency is missing.");
if (string.IsNullOrWhiteSpace(stripeWebhook))
    throw new InvalidOperationException("Stripe webhook secret is missing.");

StripeConfiguration.ApiKey = stripeSecret;

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(paymentDbConnectionString));
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        cfg.Host(rabbitHost);
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddScoped<IPaymentManagementService, PaymentManagementService>();
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