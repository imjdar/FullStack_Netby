using Inventory.Products.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Products.Api.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (context.Productos.Any())
            {
                return;
            }

            var products = new Product[]
            {
                new Product { Name = "MacBook Air M2", Description = "Laptop Apple de 13 pulgadas, 8GB RAM, 256GB SSD, color Medianoche.", Category = "Laptops", Price = 1199.99m, StockQuantity = 15, ImageUrl = "/images/default.png" },
                new Product { Name = "Dell XPS 15", Description = "Laptop potente para diseño y desarrollo, i7, 16GB RAM, 512GB SSD.", Category = "Laptops", Price = 1450.50m, StockQuantity = 8, ImageUrl = "/images/default.png" },
                new Product { Name = "Logitech MX Master 3S", Description = "Mouse inalámbrico ergonómico de alta precisión con scroll magnético.", Category = "Accesorios", Price = 99.00m, StockQuantity = 25, ImageUrl = "/images/default.png" },
                new Product { Name = "Keychron K2 V2", Description = "Teclado mecánico wireless 75% con switches Gateron Brown.", Category = "Accesorios", Price = 89.99m, StockQuantity = 12, ImageUrl = "/images/default.png" },
                new Product { Name = "LG UltraWide 34\"", Description = "Monitor panorámico 21:9 para máxima productividad multitarea.", Category = "Monitores", Price = 450.00m, StockQuantity = 10, ImageUrl = "/images/default.png" },
                new Product { Name = "Sony WH-1000XM5", Description = "Auriculares inalámbricos con cancelación de ruido líder en la industria.", Category = "Audio", Price = 349.99m, StockQuantity = 20, ImageUrl = "/images/default.png" },
                new Product { Name = "Silla Ergonómica Pro", Description = "Silla de oficina con soporte lumbar ajustable y malla traspirable.", Category = "Muebles", Price = 280.00m, StockQuantity = 15, ImageUrl = "/images/default.png" },
                new Product { Name = "Standing Desk Motorizado", Description = "Escritorio con ajuste de altura eléctrico y memoria de posiciones.", Category = "Muebles", Price = 399.00m, StockQuantity = 5, ImageUrl = "/images/default.png" },
                new Product { Name = "iPad Air 5", Description = "Tablet potente con chip M1, pantalla Liquid Retina de 10.9 pulgadas.", Category = "Tablets", Price = 599.00m, StockQuantity = 18, ImageUrl = "/images/default.png" },
                new Product { Name = "Kindle Paperwhite", Description = "Lector de libros electrónicos con luz ajustable y pantalla de 6.8\".", Category = "Gadgets", Price = 139.99m, StockQuantity = 30, ImageUrl = "/images/default.png" }
            };

            context.Productos.AddRange(products);
            context.SaveChanges();
        }
    }
}
