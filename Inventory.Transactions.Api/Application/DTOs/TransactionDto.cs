using System.ComponentModel.DataAnnotations;
using Inventory.Transactions.Api.Domain.Entities;

namespace Inventory.Transactions.Api.Application.DTOs
{
    public class TransactionCreateDto
    {
        [Required(ErrorMessage = "El ID del producto es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser válido.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "El tipo de transacción es obligatorio.")]
        public TransactionType Type { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "El precio unitario es obligatorio.")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio unitario debe estar entre $0.01 y $999,999.99.")]
        public decimal UnitPrice { get; set; }

        [MaxLength(500, ErrorMessage = "El detalle no puede exceder 500 caracteres.")]
        public string Detail { get; set; } = string.Empty;
    }

    public class TransactionResponseDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Detail { get; set; } = string.Empty;
    }
}
