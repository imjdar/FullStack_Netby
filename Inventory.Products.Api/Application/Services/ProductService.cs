using Microsoft.EntityFrameworkCore;
using Inventory.Products.Api.Application.DTOs;
using Inventory.Products.Api.Domain.Entities;
using Inventory.Products.Api.Infrastructure.Data;

namespace Inventory.Products.Api.Application.Services
{
    /// <summary>
    /// Capa de aplicación: Toda la lógica de negocio de productos.
    /// El controlador no accede al DbContext directamente.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<ProductService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Productos.AsNoTracking().ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateAsync(ProductCreateDto dto)
        {
            string imageUrl = "/images/default.png";

            if (dto.ArchivoImagen != null)
            {
                imageUrl = await UploadImageAsync(dto.ArchivoImagen);
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                ImageUrl = imageUrl
            };

            _context.Productos.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Producto creado: {ProductId} - {ProductName}", product.Id, product.Name);
            return product;
        }

        public async Task<Product?> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var product = await _context.Productos.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Intento de actualizar producto inexistente: {ProductId}", id);
                return null;
            }

            if (dto.NewImage != null)
            {
                product.ImageUrl = await UploadImageAsync(dto.NewImage);
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Category = dto.Category;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Producto actualizado: {ProductId} - {ProductName}", product.Id, product.Name);
            return product;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Productos.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Intento de eliminar producto inexistente: {ProductId}", id);
                return false;
            }

            _context.Productos.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Producto eliminado: {ProductId} - {ProductName}", product.Id, product.Name);
            return true;
        }

        public async Task<(bool Success, int NewStock, string Message)> UpdateStockAsync(UpdateStockDto dto)
        {
            var product = await _context.Productos.FindAsync(dto.ProductId);
            if (product == null)
            {
                return (false, 0, $"Producto con ID {dto.ProductId} no encontrado.");
            }

            int projectedStock = product.StockQuantity + dto.Change;
            if (projectedStock < 0)
            {
                _logger.LogWarning(
                    "Operación de stock rechazada para producto {ProductId}: Stock actual={CurrentStock}, Cambio={Change}, Resultado proyectado={Projected}",
                    dto.ProductId, product.StockQuantity, dto.Change, projectedStock);
                return (false, product.StockQuantity, $"Stock insuficiente. Actual: {product.StockQuantity}, Cambio solicitado: {dto.Change}.");
            }

            product.StockQuantity = projectedStock;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Stock actualizado para producto {ProductId}: {OldStock} -> {NewStock}",
                dto.ProductId, projectedStock - dto.Change, projectedStock);

            return (true, product.StockQuantity, "Stock actualizado correctamente.");
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            string wwwRootPath = _environment.WebRootPath
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            string folderPath = Path.Combine(wwwRootPath, "images");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Imagen subida: {FileName}", fileName);
            return $"/images/{fileName}";
        }
    }
}
