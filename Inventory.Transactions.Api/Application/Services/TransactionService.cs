using Microsoft.EntityFrameworkCore;
using Inventory.Transactions.Api.Application.DTOs;
using Inventory.Transactions.Api.Domain.Entities;
using Inventory.Transactions.Api.Infra.Data;
using System.Text.Json;
using System.Text;

namespace Inventory.Transactions.Api.Application.Services
{
    /// <summary>
    /// Capa de aplicación: Lógica de negocio de transacciones.
    /// Implementa flujos transaccionales completos (todo o nada).
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TransactionService> _logger;

        private string ProductsApiUrl => _configuration["Services:ProductsUrl"]
            ?? throw new InvalidOperationException("La configuración 'Services:ProductsUrl' no está definida.");

        public TransactionService(
            ApplicationDbContext context,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TransactionService> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<TransactionResponseDto>> GetTransactionsAsync(DateTime? startDate, DateTime? endDate, int? type)
        {
            var query = _context.Transactions.AsNoTracking().AsQueryable();

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value.AddDays(1));

            if (type.HasValue)
                query = query.Where(t => (int)t.Type == type.Value);

            var transactions = await query.OrderByDescending(t => t.Date).ToListAsync();

            var productIds = transactions.Select(t => t.ProductId).Distinct();
            var productNames = new Dictionary<int, string>();

            foreach (var productId in productIds)
            {
                productNames[productId] = await GetProductNameAsync(productId);
            }

            return transactions.Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                Date = t.Date,
                Type = t.Type.ToString(),
                ProductId = t.ProductId,
                ProductName = productNames.GetValueOrDefault(t.ProductId, $"Producto #{t.ProductId}"),
                Quantity = t.Quantity,
                UnitPrice = t.UnitPrice,
                TotalPrice = t.TotalPrice,
                Detail = t.Detail
            });
        }

        public async Task<TransactionResponseDto?> CreateTransactionAsync(TransactionCreateDto dto)
        {
            // ── 1. Validar stock si es Venta ──
            if (dto.Type == TransactionType.Venta)
            {
                var currentStock = await GetProductStockAsync(dto.ProductId);
                if (currentStock < dto.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente para la venta. Disponible: {currentStock}, Solicitado: {dto.Quantity}.");
                }
            }

            // ── 2. Registrar la transacción ──
            var transaction = new Transaction
            {
                Date = DateTime.UtcNow,
                Type = dto.Type,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                TotalPrice = dto.Quantity * dto.UnitPrice,
                Detail = dto.Detail
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // ── 3. Actualizar stock en Products API ──
            int stockChange = dto.Type == TransactionType.Compra ? dto.Quantity : -dto.Quantity;
            var stockUpdateSuccess = await UpdateProductStockAsync(dto.ProductId, stockChange);

            if (!stockUpdateSuccess)
            {
                // Flujo completo: si falla el stock, revertimos la transacción
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                _logger.LogError(
                    "Transacción {TransactionId} revertida: fallo al actualizar stock del producto {ProductId}",
                    transaction.Id, dto.ProductId);

                throw new InvalidOperationException(
                    "Error al sincronizar el stock. La transacción fue revertida.");
            }

            _logger.LogInformation(
                "Transacción {TransactionId} creada: {Type} x{Quantity} del producto {ProductId}, Total: ${Total}",
                transaction.Id, dto.Type, dto.Quantity, dto.ProductId, transaction.TotalPrice);

            var productName = await GetProductNameAsync(dto.ProductId);

            return new TransactionResponseDto
            {
                Id = transaction.Id,
                Date = transaction.Date,
                Type = transaction.Type.ToString(),
                ProductId = transaction.ProductId,
                ProductName = productName,
                Quantity = transaction.Quantity,
                UnitPrice = transaction.UnitPrice,
                TotalPrice = transaction.TotalPrice,
                Detail = transaction.Detail
            };
        }

        // ── Métodos privados de comunicación inter-servicio ──

        private async Task<string> GetProductNameAsync(int productId)
        {
            try
            {
                var url = $"{ProductsApiUrl}/api/Products/{productId}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return $"Producto #{productId}";

                var json = await response.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<JsonElement>(json);
                return product.GetProperty("name").GetString() ?? $"Producto #{productId}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener el nombre del producto {ProductId}", productId);
                return $"Producto #{productId}";
            }
        }

        private async Task<int> GetProductStockAsync(int productId)
        {
            var url = $"{ProductsApiUrl}/api/Products/{productId}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al consultar stock del producto {ProductId}: {StatusCode} - {Body}",
                    productId, response.StatusCode, errorBody);
                throw new InvalidOperationException($"No se pudo validar el stock del producto {productId}.");
            }

            var json = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<JsonElement>(json);
            return product.GetProperty("stockQuantity").GetInt32();
        }

        private async Task<bool> UpdateProductStockAsync(int productId, int change)
        {
            try
            {
                var url = $"{ProductsApiUrl}/api/Products/update-stock";
                var payload = new { ProductId = productId, Change = change };
                var content = new StringContent(
                    JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Fallo al actualizar stock: Producto={ProductId}, Cambio={Change}, Status={Status}, Body={Body}",
                        productId, change, response.StatusCode, errorBody);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al comunicarse con Products API para actualizar stock del producto {ProductId}", productId);
                return false;
            }
        }
    }
}
