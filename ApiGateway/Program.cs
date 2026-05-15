using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("JWT secret is missing. Set Jwt__Secret environment variable.");

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Content("""
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <title>ECommerce API Gateway</title>
  <style>
    *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
    body { font-family: 'Segoe UI', sans-serif; background: #0a0a0f; color: #e2e8f0; min-height: 100vh; padding: 3rem 2rem; }
    header { text-align: center; margin-bottom: 3rem; }
    header h1 { font-size: 2rem; font-weight: 700; color: #f8fafc; letter-spacing: -0.5px; }
    header p { margin-top: 0.5rem; color: #64748b; font-size: 0.95rem; }
    .port-badge { display: inline-block; margin-top: 0.75rem; background: #1e293b; border: 1px solid #334155; border-radius: 999px; padding: 0.25rem 1rem; font-size: 0.8rem; color: #94a3b8; }
    .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1.25rem; max-width: 1100px; margin: 0 auto; }
    .card { background: #0f172a; border: 1px solid #1e293b; border-radius: 12px; padding: 1.5rem; transition: border-color 0.2s; }
    .card:hover { border-color: #334155; }
    .card-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; }
    .dot { width: 8px; height: 8px; border-radius: 50%; background: #22c55e; box-shadow: 0 0 6px #22c55e; flex-shrink: 0; }
    .card-title { font-size: 1rem; font-weight: 600; color: #f1f5f9; }
    .route { font-size: 0.78rem; font-family: 'Consolas', monospace; background: #1e293b; color: #f472b6; padding: 0.3rem 0.65rem; border-radius: 6px; display: inline-block; margin-bottom: 0.5rem; }
    .dest { font-size: 0.75rem; color: #475569; margin-top: 0.75rem; }
    .dest span { color: #60a5fa; }
    .lock { display: inline-block; margin-top: 0.6rem; font-size: 0.72rem; color: #f59e0b; background: #1c1200; border: 1px solid #78350f; border-radius: 4px; padding: 0.15rem 0.5rem; }
    .open { display: inline-block; margin-top: 0.6rem; font-size: 0.72rem; color: #4ade80; background: #052e16; border: 1px solid #166534; border-radius: 4px; padding: 0.15rem 0.5rem; }
    .scalar-link { display: inline-block; margin-top: 0.75rem; font-size: 0.75rem; color: #818cf8; text-decoration: none; border: 1px solid #312e81; background: #0f0a2e; border-radius: 4px; padding: 0.2rem 0.6rem; transition: background 0.15s; }
    .scalar-link:hover { background: #1e1b4b; color: #a5b4fc; }
    footer { text-align: center; margin-top: 3rem; color: #1e293b; font-size: 0.75rem; }
  </style>
</head>
<body>
  <header>
    <h1>⚡ ECommerce API Gateway</h1>
    <p>All traffic enters here. Services are routed and JWT is validated at this layer.</p>
    <span class="port-badge">:5000 → internal :8080</span>
  </header>

  <div class="grid">
    <div class="card">
      <div class="card-header"><span class="dot"></span><span class="card-title">UserService</span></div>
      <span class="route">/api/auth/{**}</span>
      <div class="dest">→ <span>userservice:8080</span></div>
      <span class="open">🔓 public</span>
      <br><a class="scalar-link" href="http://localhost:5124/scalar/v1" target="_blank">Open Scalar ↗</a>
    </div>
    <div class="card">
      <div class="card-header"><span class="dot"></span><span class="card-title">ProductService</span></div>
      <span class="route">/api/products/{**}</span>
      <div class="dest">→ <span>productservice:8080</span></div>
      <span class="open">🔓 public</span>
      <br><a class="scalar-link" href="http://localhost:5212/scalar/v1" target="_blank">Open Scalar ↗</a>
    </div>
    <div class="card">
      <div class="card-header"><span class="dot"></span><span class="card-title">OrderService</span></div>
      <span class="route">/api/orders/{**}</span>
      <div class="dest">→ <span>orderservice:8080</span></div>
      <span class="lock">🔒 JWT required</span>
      <br><a class="scalar-link" href="http://localhost:5313/scalar/v1" target="_blank">Open Scalar ↗</a>
    </div>
    <div class="card">
      <div class="card-header"><span class="dot"></span><span class="card-title">PaymentService</span></div>
      <span class="route">/api/payments/{**}</span>
      <div class="dest">→ <span>paymentservice:8080</span></div>
      <span class="lock">🔒 JWT required</span>
      <br><a class="scalar-link" href="http://localhost:5314/scalar/v1" target="_blank">Open Scalar ↗</a>
    </div>
    <div class="card">
      <div class="card-header"><span class="dot"></span><span class="card-title">InventoryService</span></div>
      <span class="route">/api/inventory/{**}</span>
      <div class="dest">→ <span>inventoryservice:8080</span></div>
      <span class="lock">🔒 JWT required</span>
      <br><a class="scalar-link" href="http://localhost:5315/scalar/v1" target="_blank">Open Scalar ↗</a>
    </div>
  </div>

  <footer>ECommerce Platform · YARP Reverse Proxy · JWT validated at gateway</footer>
</body>
</html>
""", "text/html"));

app.MapReverseProxy();

app.Run();
