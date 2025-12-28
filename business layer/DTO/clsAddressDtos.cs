// ملف: AddressDtos.cs
namespace Business_layer.Dtos
{
    // لعرض عنوان (الإخراج)
    public class AddressResponseDto
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public bool IsDefault { get; set; }
    }

    // لإضافة عنوان جديد (الإدخال)
    public class AddAddressRequestDto
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public bool IsDefault { get; set; } = false;
    }

    // لتحديث عنوان موجود
    public class UpdateAddressRequestDto
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public bool IsDefault { get; set; }
    }
}