// File: AuditLogDtos.cs
namespace Business_layer.Dtos
{
    public class AuditLogResponseDto
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}