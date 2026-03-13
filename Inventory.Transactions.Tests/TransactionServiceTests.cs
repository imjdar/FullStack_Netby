using System.Net;
using System.Text;
using System.Text.Json;
using Inventory.Transactions.Api.Application.DTOs;
using Inventory.Transactions.Api.Application.Services;
using Inventory.Transactions.Api.Domain.Entities;
using Inventory.Transactions.Api.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Inventory.Transactions.Tests
{
    public class TransactionServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<TransactionService>> _mockLogger;

        public TransactionServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["Services:ProductsUrl"]).Returns("http://test-products-api");
            _mockLogger = new Mock<ILogger<TransactionService>>();
        }

        public void Dispose() => _context.Dispose();

        private TransactionService CreateService(HttpResponseMessage? defaultResponse = null)
        {
            var response = defaultResponse ?? new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(
                    new { name = "Test Product", stockQuantity = 100, price = 50.0 }))
            };

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHandler.Object);
            return new TransactionService(_context, httpClient, _mockConfig.Object, _mockLogger.Object);
        }

        // ── CREATE: Compra ──

        [Fact]
        public async Task CreateTransaction_Compra_CalculatesTotalCorrectly()
        {
            var service = CreateService();
            var dto = new TransactionCreateDto
            {
                ProductId = 1,
                Type = TransactionType.Compra,
                Quantity = 5,
                UnitPrice = 10.50m,
                Detail = "Compra de prueba"
            };

            var result = await service.CreateTransactionAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(52.50m, result.TotalPrice);
            Assert.Equal("Compra", result.Type);
            Assert.Equal(1, _context.Transactions.Count());
        }

        [Fact]
        public async Task CreateTransaction_Compra_SetsUtcDate()
        {
            var service = CreateService();
            var dto = new TransactionCreateDto
            {
                ProductId = 1,
                Type = TransactionType.Compra,
                Quantity = 1,
                UnitPrice = 100m
            };

            var result = await service.CreateTransactionAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(DateTimeKind.Utc, result.Date.Kind);
        }

        // ── CREATE: Venta ──

        [Fact]
        public async Task CreateTransaction_Venta_WithSufficientStock_Succeeds()
        {
            var service = CreateService();
            var dto = new TransactionCreateDto
            {
                ProductId = 1,
                Type = TransactionType.Venta,
                Quantity = 5,
                UnitPrice = 20m
            };

            var result = await service.CreateTransactionAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("Venta", result.Type);
            Assert.Equal(100m, result.TotalPrice);
        }

        [Fact]
        public async Task CreateTransaction_Venta_InsufficientStock_ThrowsException()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(
                    new { name = "Low Stock", stockQuantity = 2, price = 10.0 }))
            };

            var service = CreateService(response);
            var dto = new TransactionCreateDto
            {
                ProductId = 1,
                Type = TransactionType.Venta,
                Quantity = 10,
                UnitPrice = 10m
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateTransactionAsync(dto));

            Assert.Contains("insuficiente", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, _context.Transactions.Count());
        }

        // ── CREATE: Flujo transaccional (reversión) ──

        [Fact]
        public async Task CreateTransaction_WhenStockUpdateFails_RevertsTransaction()
        {
            // Configuración: GET exitoso pero POST (update-stock) falla
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
                {
                    // GET retorna producto válido
                    if (request.Method == HttpMethod.Get)
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(JsonSerializer.Serialize(
                                new { name = "Test", stockQuantity = 100, price = 10.0 }))
                        };
                    }
                    // POST (update-stock) falla
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var service = new TransactionService(_context, httpClient, _mockConfig.Object, _mockLogger.Object);

            var dto = new TransactionCreateDto
            {
                ProductId = 1,
                Type = TransactionType.Compra,
                Quantity = 5,
                UnitPrice = 10m
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateTransactionAsync(dto));

            // Verifica que la transacción fue revertida
            Assert.Equal(0, _context.Transactions.Count());
        }

        // ── GET: Filtros ──

        [Fact]
        public async Task GetTransactions_NoFilters_ReturnsAll()
        {
            _context.Transactions.AddRange(
                new Transaction { Date = DateTime.UtcNow, Type = TransactionType.Compra, ProductId = 1, Quantity = 1, UnitPrice = 10, TotalPrice = 10 },
                new Transaction { Date = DateTime.UtcNow, Type = TransactionType.Venta, ProductId = 2, Quantity = 2, UnitPrice = 20, TotalPrice = 40 }
            );
            await _context.SaveChangesAsync();

            var service = CreateService();
            var result = await service.GetTransactionsAsync(null, null, null);

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetTransactions_FilterByType_ReturnsFiltered()
        {
            _context.Transactions.AddRange(
                new Transaction { Date = DateTime.UtcNow, Type = TransactionType.Compra, ProductId = 1, Quantity = 1, UnitPrice = 10, TotalPrice = 10 },
                new Transaction { Date = DateTime.UtcNow, Type = TransactionType.Venta, ProductId = 2, Quantity = 2, UnitPrice = 20, TotalPrice = 40 },
                new Transaction { Date = DateTime.UtcNow, Type = TransactionType.Compra, ProductId = 3, Quantity = 3, UnitPrice = 30, TotalPrice = 90 }
            );
            await _context.SaveChangesAsync();

            var service = CreateService();
            var result = await service.GetTransactionsAsync(null, null, (int)TransactionType.Compra);

            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.Equal("Compra", t.Type));
        }

        // ── GET: Resolución de nombres de productos ──

        [Fact]
        public async Task GetTransactions_ResolvesProductNames()
        {
            _context.Transactions.Add(
                new Transaction { Date = DateTime.UtcNow, Type = TransactionType.Compra, ProductId = 1, Quantity = 1, UnitPrice = 10, TotalPrice = 10 }
            );
            await _context.SaveChangesAsync();

            var service = CreateService();
            var result = (await service.GetTransactionsAsync(null, null, null)).ToList();

            Assert.Single(result);
            Assert.Equal("Test Product", result[0].ProductName);
        }
    }
}
