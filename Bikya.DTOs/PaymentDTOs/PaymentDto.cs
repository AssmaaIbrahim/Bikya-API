using Bikya.Data.Enums;
using System;

namespace Bikya.DTOs.PaymentDTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public int UserId { get; set; }
        public int? OrderId { get; set; }
        public PaymentGateway Gateway { get; set; }
        public string? GatewayReference { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public PaymentStatus Status { get; set; }
        // Stripe: client secret to complete payment on frontend
        public string? ClientSecret { get; set; }
        // PayPal: payment approval URL for user to complete payment
        public string? PaymentUrl { get; set; }
    }
} 