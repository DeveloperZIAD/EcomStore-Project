// File: OrderItemDtos.cs
namespace Business_layer.Dtos
{
    public class OrderItemResponseDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }

    // Typically used internally during order creation (e.g., from cart or guest checkout)
    public class OrderItemRequestDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }
}