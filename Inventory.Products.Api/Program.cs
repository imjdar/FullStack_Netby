using Inventory.Products.Api.Application.Services;
using Inventory.Products.Api.Infrastructure.Data; 
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Capa de Infraestructura
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Capa de Aplicación
builder.Services.AddScoped<IProductService, ProductService>();

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

//Configuración del Pipeline de HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Mapea el JSON
    app.MapScalarApiReference();
}
app.UseStaticFiles();

//Aplicar la política de CORS
app.UseCors("AllowReactApp");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
