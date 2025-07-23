using Bikya.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bikya.DTOs.PaymentDTOs
{
    public class CreatePaymentDto
    {
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public int UserId { get; set; }
        public int? OrderId { get; set; }
        [Required]
        public PaymentGateway Gateway { get; set; }
        public string? Description { get; set; }
    }
} 