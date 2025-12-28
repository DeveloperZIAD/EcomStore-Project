// File: CategoryDtos.cs
namespace Business_layer.Dtos
{
    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddCategoryRequestDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UpdateCategoryRequestDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}