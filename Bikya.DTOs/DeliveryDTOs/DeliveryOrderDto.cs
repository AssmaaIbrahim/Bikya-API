using Bikya.Data.Enums;

namespace Bikya.DTOs.DeliveryDTOs
{
    public class DeliveryOrderDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        
        // Shipping Information
        public string RecipientName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public ShippingStatus ShippingStatus { get; set; }
        
        // Customer Information
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerPhone { get; set; }
    }
}

