// File: PaymentDtos.cs
namespace Business_layer.Dtos
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }          // pending, completed, failed
        public string TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdatePaymentStatusRequestDto
    {
        public string NewStatus { get; set; }       // pending, completed, failed
        public string TransactionId { get; set; }   // Optional, usually from payment gateway
    }
}