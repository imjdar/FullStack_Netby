using Inventory.Products.Api.Application.DTOs;
using Inventory.Products.Api.Application.Services;
using Inventory.Products.Api.Domain.Entities;
using Inventory.Products.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Inventory.Products.Tests
{
    public class ProductServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ProductService _service;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<ILogger<ProductService>> _mockLogger;
        private readonly string _tempPath;

        public ProductServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockLogger = new Mock<ILogger<ProductService>>();

            _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _mockEnvironment.Setup(m => m.WebRootPath).Returns(_tempPath);

            _service = new ProductService(_context, _mockEnvironment.Object, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
            if (Directory.Exists(_tempPath)) Directory.Delete(_tempPath, true);
        }

        // ── CREATE ──

        [Fact]
        public async Task CreateAsync_WithValidDto_ReturnsProduct()
        {
            var dto = new ProductCreateDto
            {
                Name = "Laptop Test",
                Description = "Laptop de prueba",
                Category = "Electrónica",
                Price = 1200.00m,
                StockQuantity = 10
            };

            var result = await _service.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("Laptop Test", result.Name);
            Assert.Equal(1200.00m, result.Price);
            Assert.Equal(10, result.StockQuantity);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task CreateAsync_WithImage_UploadsAndSetsUrl()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("product.png");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            var dto = new ProductCreateDto
            {
                Name = "Con Imagen",
                Category = "Test",
                Price = 50m,
                StockQuantity = 5,
                ArchivoImagen = mockFile.Object
            };

            var result = await _service.CreateAsync(dto);

            Assert.NotNull(result.ImageUrl);
            Assert.Contains("/images/", result.ImageUrl);
        }

        [Fact]
        public async Task CreateAsync_WithoutImage_UsesDefaultUrl()
        {
            var dto = new ProductCreateDto
            {
                Name = "Sin Imagen",
                Category = "Test",
                Price = 30m,
                StockQuantity = 3
            };

            var result = await _service.CreateAsync(dto);

            Assert.Equal("/images/default.png", result.ImageUrl);
        }

        // ── READ ──

        [Fact]
        public async Task GetAllAsync_ReturnsAllProducts()
        {
            _context.Productos.AddRange(
                new Product { Name = "A", Price = 10, StockQuantity = 1, Category = "X" },
                new Product { Name = "B", Price = 20, StockQuantity = 2, Category = "Y" }
            );
            await _context.SaveChangesAsync();

            var result = await _service.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ExistingProduct_ReturnsProduct()
        {
            var product = new Product { Name = "Existente", Price = 100, StockQuantity = 5, Category = "Test" };
            _context.Productos.Add(product);
            await _context.SaveChangesAsync();

            var result = await _service.GetByIdAsync(product.Id);

            Assert.NotNull(result);
            Assert.Equal("Existente", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingProduct_ReturnsNull()
        {
            var result = await _service.GetByIdAsync(999);

            Assert.Null(result);
        }

        // ── UPDATE ──

        [Fact]
        public async Task UpdateAsync_ExistingProduct_UpdatesFields()
        {
            var product = new Product { Name = "Original", Price = 50, StockQuantity = 10, Category = "Old" };
            _context.Productos.Add(product);
            await _context.SaveChangesAsync();

            var dto = new ProductUpdateDto
            {
                Name = "Actualizado",
                Category = "New",
                Price = 75,
                StockQuantity = 15,
                Description = "Desc"
            };

            var result = await _service.UpdateAsync(product.Id, dto);

            Assert.NotNull(result);
            Assert.Equal("Actualizado", result.Name);
            Assert.Equal(75, result.Price);
            Assert.Equal(15, result.StockQuantity);
        }

        [Fact]
        public async Task UpdateAsync_NonExisting_ReturnsNull()
        {
            var dto = new ProductUpdateDto { Name = "X", Category = "X", Price = 1, StockQuantity = 1 };

            var result = await _service.UpdateAsync(999, dto);

            Assert.Null(result);
        }

        // ── DELETE ──

        [Fact]
        public async Task DeleteAsync_ExistingProduct_ReturnsTrue()
        {
            var product = new Product { Name = "Borrable", Price = 10, StockQuantity = 1, Category = "Test" };
            _context.Productos.Add(product);
            await _context.SaveChangesAsync();

            var result = await _service.DeleteAsync(product.Id);

            Assert.True(result);
            Assert.Null(await _context.Productos.FindAsync(product.Id));
        }

        [Fact]
        public async Task DeleteAsync_NonExisting_ReturnsFalse()
        {
            var result = await _service.DeleteAsync(999);

            Assert.False(result);
        }

        // ── UPDATE STOCK ──

        [Fact]
        public async Task UpdateStockAsync_PositiveChange_IncreasesStock()
        {
            var product = new Product { Name = "Stock Test", Price = 10, StockQuantity = 10, Category = "Test" };
            _context.Productos.Add(product);
            await _context.SaveChangesAsync();

            var (success, newStock, _) = await _service.UpdateStockAsync(
                new UpdateStockDto { ProductId = product.Id, Change = 5 });

            Assert.True(success);
            Assert.Equal(15, newStock);
        }

        [Fact]
        public async Task UpdateStockAsync_NegativeChange_DecreasesStock()
        {
            var product = new Product { Name = "Stock Test", Price = 10, StockQuantity = 10, Category = "Test" };
            _context.Productos.Add(product);
            await _context.SaveChangesAsync();

            var (success, newStock, _) = await _service.UpdateStockAsync(
                new UpdateStockDto { ProductId = product.Id, Change = -3 });

            Assert.True(success);
            Assert.Equal(7, newStock);
        }

        [Fact]
        public async Task UpdateStockAsync_InsufficientStock_ReturnsFalse()
        {
            var product = new Product { Name = "Stock Bajo", Price = 10, StockQuantity = 2, Category = "Test" };
            _context.Productos.Add(product);
            await _context.SaveChangesAsync();

            var (success, newStock, message) = await _service.UpdateStockAsync(
                new UpdateStockDto { ProductId = product.Id, Change = -5 });

            Assert.False(success);
            Assert.Equal(2, newStock);
            Assert.Contains("insuficiente", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateStockAsync_NonExistingProduct_ReturnsFalse()
        {
            var (success, _, message) = await _service.UpdateStockAsync(
                new UpdateStockDto { ProductId = 999, Change = 1 });

            Assert.False(success);
            Assert.Contains("no encontrado", message, StringComparison.OrdinalIgnoreCase);
        }

        // ── UPLOAD IMAGE ──

        [Fact]
        public async Task UploadImageAsync_CreatesDirectoryAndFile()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.jpg");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            var result = await _service.UploadImageAsync(mockFile.Object);

            Assert.Contains("/images/", result);
            Assert.True(Directory.Exists(Path.Combine(_tempPath, "images")));
        }
    }
}
