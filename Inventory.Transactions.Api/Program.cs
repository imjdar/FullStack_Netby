using Microsoft.EntityFrameworkCore;
using Inventory.Transactions.Api.Infra.Data;
using Inventory.Transactions.Api.Application.Services;
using Inventory.Transactions.Api.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Capa de Infraestructura ──
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La cadena de conexión 'DB_CONNECTION' no está configurada.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ── Capa de Aplicación ──
builder.Services.AddScoped<ITransactionService, TransactionService>();

// ── HttpClient con timeout para comunicación inter-servicio ──
builder.Services.AddHttpClient<TransactionService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient();

// ── Configuración de CORS ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// NOTA: Las migraciones automáticas se han removido para usar scripts SQL de inicialización externos.

// ── Pipeline HTTP ──
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
