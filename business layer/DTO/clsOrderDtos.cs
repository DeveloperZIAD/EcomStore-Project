// File: OrderDtos.cs
namespace Business_layer.Dtos
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? AddressId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderSummaryResponseDto  // For admin lists
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FullAddress { get; set; }
    }

    public class UpdateOrderStatusRequestDto
    {
        public string NewStatus { get; set; }
    }
}