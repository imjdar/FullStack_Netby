namespace Inventory.Products.Api.Application.DTOs
{
    public class ProductBaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
