using Bikya.Data.Enums;
using System;

namespace Bikya.Data.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public int UserId { get; set; }
        public int? OrderId { get; set; }
        public PaymentGateway Gateway { get; set; }
        public string? GatewayReference { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    }
} 