using Microsoft.AspNetCore.Http;

namespace Inventory.Products.Api.Application.Services
{
    public interface IProductService
    {
        Task<string> UploadImage(IFormFile file);
    }
    public class ProductService : IProductService
    {
        public async Task<string> UploadImage(IFormFile file)
        {
            // Ruta del volumen en Docker
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Generamos un nombre único
            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{fileName}";
        }
    }
}
