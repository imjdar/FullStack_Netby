using System.ComponentModel.DataAnnotations;

namespace Inventory.Products.Api.Application.DTOs
{
    public class UpdateStockDto
    {
        [Required(ErrorMessage = "El ID del producto es obligatorio.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "El cambio de stock es obligatorio.")]
        public int Change { get; set; }
    }
}
