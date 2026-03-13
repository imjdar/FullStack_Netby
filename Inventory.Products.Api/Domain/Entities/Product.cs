using System.ComponentModel.DataAnnotations;

namespace Inventory.Products.Api.Domain.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [MaxLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
        public string? Description { get; set; }

        [MaxLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres.")]
        public string? Category { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int StockQuantity { get; set; }

        public string? ImageUrl { get; set; }
    }
}
