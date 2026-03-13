using Inventory.Products.Api.Application.DTOs;
using Inventory.Products.Api.Domain.Entities;

namespace Inventory.Products.Api.Application.Services
{
    /// <summary>
    /// Contrato de la capa de aplicación para la gestión de productos.
    /// </summary>
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateAsync(ProductCreateDto dto);
        Task<Product?> UpdateAsync(int id, ProductUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<(bool Success, int NewStock, string Message)> UpdateStockAsync(UpdateStockDto dto);
        Task<string> UploadImageAsync(IFormFile file);
    }
}
