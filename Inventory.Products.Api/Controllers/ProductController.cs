using Microsoft.AspNetCore.Mvc;
using Inventory.Products.Api.Application.DTOs;
using Inventory.Products.Api.Application.Services;
using Inventory.Products.Api.Infrastructure.Data;
using Inventory.Products.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Products.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; //La bdd conexion
        private readonly IProductService _productService; // Logica de Negocio (ej. subir imagen)

        public ProductsController(ApplicationDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        //(GET)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Productos.ToListAsync();
            return Ok(products);
        }

        //(GET por id)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Productos.FindAsync(id);
            if (product == null) return NotFound(new { message = "Producto no encontrado" });

            return Ok(product);
        }

        //(Post)
        [HttpPost]
        [Consumes("multipart/form-data")] // Necesario para recibir archivos
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto)
        {
            // Manejo de la imagen
            string imageUrl = "/images/default.png";
            if (dto.ArchivoImagen != null)
            {
                imageUrl = await _productService.UploadImage(dto.ArchivoImagen);
            }

            // Mapeo de DTO a Entidad
            var newProduct = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                ImageUrl = imageUrl
            };

            _context.Productos.Add(newProduct);
            await _context.SaveChangesAsync();

            // Retorna un 201 Created y la ruta para ver el nuevo producto
            return CreatedAtAction(nameof(GetById), new { id = newProduct.Id }, newProduct);
        }

        //(PUT)
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductUpdateDto dto)
        {
            var product = await _context.Productos.FindAsync(id);
            if (product == null) return NotFound();

            // Si viene imagen nueva, se sube; si no, se queda el valor que estaba
            if (dto.NewImage != null)
            {
                product.ImageUrl = await _productService.UploadImage(dto.NewImage);
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.Category = dto.Category;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        //(DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Productos.FindAsync(id);
            if (product == null) return NotFound();

            _context.Productos.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Producto {id} eliminado correctamente" });
        }
    }
}
