using Microsoft.AspNetCore.Http;
namespace Inventory.Products.Api.Application.DTOs
{
    public class ProductCreateDto : ProductBaseDto
    {
        public IFormFile? ArchivoImagen { get; set; } // Nombre para creación
    }
}
