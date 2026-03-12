namespace Inventory.Products.Api.Application.DTOs
{
    public class ProductUpdateDto : ProductBaseDto
    {
        public IFormFile? NewImage { get; set; } // Nombre para actualización
    }
}
