// File: FullOrderDetailsDto.cs (في Business_layer أو Dtos)
using Data_layer;

namespace Business_layer
{
    public class FullOrderDetailsDto
    {
        public clsorder Order { get; set; }
        public List<clsorderitem> Items { get; set; } = new List<clsorderitem>();
        public clspayment Payment { get; set; }
        public clsaddressesdb Address { get; set; }
        public clsuser User { get; set; }
    }
}