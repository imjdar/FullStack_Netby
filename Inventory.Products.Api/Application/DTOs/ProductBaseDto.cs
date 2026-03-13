using System.ComponentModel.DataAnnotations;

namespace Inventory.Products.Api.Application.DTOs
{
    public class ProductBaseDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres.")]
        public string? Category { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre $0.01 y $999,999.99.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "El stock inicial es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int StockQuantity { get; set; }
    }
}
