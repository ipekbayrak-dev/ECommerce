using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentService.Consumers;
using PaymentService.Data;
using PaymentService.Services;
using Scalar.AspNetCore;
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

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("JWT secret is missing. Set Jwt__Secret environment variable.");

StripeConfiguration.ApiKey = stripeSecret;

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(paymentDbConnectionString));
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
builder.Services.AddScoped<IPaymentManagementService, PaymentManagementService>();
builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

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

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Content("""
<html><body style="font-family:sans-serif;padding:2rem;background:#0f0f0f;color:#fff">
<h2>PaymentService is running</h2>
<a href="/scalar/v1" style="color:#60a5fa">Open Scalar UI</a>
</body></html>
""", "text/html"));

app.MapControllers();
app.Run();