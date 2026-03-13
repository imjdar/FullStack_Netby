using Microsoft.AspNetCore.Mvc;
using Inventory.Products.Api.Application.DTOs;
using Inventory.Products.Api.Application.Services;

namespace Inventory.Products.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = $"Producto con ID {id} no encontrado." });

            return Ok(product);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.UpdateAsync(id, dto);
            if (product == null)
                return NotFound(new { message = $"Producto con ID {id} no encontrado." });

            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _productService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Producto con ID {id} no encontrado." });

            return Ok(new { message = $"Producto {id} eliminado correctamente." });
        }

        [HttpPost("update-stock")]
        public async Task<IActionResult> UpdateStock([FromBody] UpdateStockDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, newStock, message) = await _productService.UpdateStockAsync(dto);

            if (!success)
            {
                _logger.LogWarning("UpdateStock fallido: {Message}", message);
                return BadRequest(new { message });
            }

            return Ok(new { message, newStock });
        }
    }
}
