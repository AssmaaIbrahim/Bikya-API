using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bikya.Data.Enums;
using Bikya.Data.Models;
using Bikya.Data.Repositories.Interfaces;
using Bikya.DTOs.PaymentDTOs;
using Bikya.Services.Interfaces;
using System;
using Bikya.Data.Response;
using Stripe;

namespace Bikya.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IWalletRepository walletRepository,
            ITransactionRepository transactionRepository)
        {
            _paymentRepository = paymentRepository;
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<ApiResponse<PaymentDto>> CreatePaymentAsync(CreatePaymentDto dto)
        {
            try
            {
                var payment = new Payment
                {
                    Amount = dto.Amount,
                    UserId = dto.UserId,
                    OrderId = dto.OrderId,
                    Gateway = dto.Gateway,
                    Description = dto.Description,
                    Status = PaymentStatus.Pending
                };

                PaymentDto? paymentDto = null;

                if (dto.Gateway == PaymentGateway.Mock)
                {
                    payment.Status = PaymentStatus.Completed;
                    payment.GatewayReference = $"MOCK-{System.Guid.NewGuid()}";
                }
                else if (dto.Gateway == PaymentGateway.Stripe)
                {
                    // Stripe integration (test mode)
                    // ملاحظة: يجب إضافة Stripe.net للمشروع وتهيئة StripeConfiguration.ApiKey في Startup
                    var options = new Stripe.PaymentIntentCreateOptions
                    {
                        Amount = (long)(dto.Amount * 100), // Stripe uses smallest currency unit
                        Currency = "egp",
                        Metadata = new Dictionary<string, string>
                        {
                            { "order_id", dto.OrderId?.ToString() ?? "" },
                            { "user_id", dto.UserId.ToString() }
                        }
                    };
                    var service = new Stripe.PaymentIntentService();
                    var paymentIntent = await service.CreateAsync(options);
                    payment.GatewayReference = paymentIntent.Id;
                    // نحتفظ بالحالة Pending حتى يتم تأكيد الدفع عبر Webhook
                    payment.Status = PaymentStatus.Pending;
                }
                else if (dto.Gateway == PaymentGateway.PayPal)
                {
                    // PayPal integration (test mode)
                    // هنا سننشئ رابط دفع وهمي (في التطبيق الحقيقي ستستخدم PayPal API)
                    payment.GatewayReference = $"PAYPAL-TEST-{System.Guid.NewGuid()}";
                    payment.Status = PaymentStatus.Pending;
                }

                var result = await _paymentRepository.AddAsync(payment);
                paymentDto = ToDto(result);

                // Stripe: أضف clientSecret
                if (dto.Gateway == PaymentGateway.Stripe)
                {
                    // Stripe integration
                    var service = new Stripe.PaymentIntentService();
                    var paymentIntent = await service.GetAsync(payment.GatewayReference);
                    paymentDto.ClientSecret = paymentIntent.ClientSecret;
                }
                // PayPal: أضف paymentUrl وهمي (في التطبيق الحقيقي ستستخدم PayPal API)
                if (dto.Gateway == PaymentGateway.PayPal)
                {
                    paymentDto.PaymentUrl = $"https://www.sandbox.paypal.com/checkoutnow?token={payment.GatewayReference}";
                }

                if (result.Status == PaymentStatus.Completed)
                {
                    var wallet = await _walletRepository.GetWalletByUserIdAsync(result.UserId);
                    if (wallet == null)
                        return ApiResponse<PaymentDto>.ErrorResponse("Wallet not found", 404);
                    if (wallet.IsLocked)
                        return ApiResponse<PaymentDto>.ErrorResponse("Wallet is locked", 403);
                    if (wallet.Balance < result.Amount)
                        return ApiResponse<PaymentDto>.ErrorResponse("Insufficient balance", 400);

                    wallet.Balance -= result.Amount;
                    await _walletRepository.UpdateWalletBalanceAsync(wallet.UserId, wallet.Balance);

                    var transaction = new Transaction
                    {
                        Amount = result.Amount,
                        Type = TransactionType.Payment,
                        Status = TransactionStatus.Completed,
                        WalletId = wallet.Id,
                        PaymentId = result.Id,
                        RelatedOrderId = result.OrderId,
                        Description = $"Payment via {result.Gateway} - {result.GatewayReference}"
                    };
                    await _transactionRepository.AddAsync(transaction);
                    await _walletRepository.SaveChangesAsync();
                }

                return ApiResponse<PaymentDto>.SuccessResponse(paymentDto, "Payment created successfully", 201);
            }
            catch (StripeException stripeEx)
            {
                // Return Stripe-specific error message
                return ApiResponse<PaymentDto>.ErrorResponse($"Stripe error: {stripeEx.Message}", 500);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Stripe/PayPal Error: " + ex.ToString());
                return ApiResponse<PaymentDto>.ErrorResponse("Technical error: " + ex.Message, 500);
            }
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            return payment == null ? null : ToDto(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByUserIdAsync(int userId)
        {
            var payments = await _paymentRepository.GetByUserIdAsync(userId);
            return payments.Select(ToDto);
        }

        private PaymentDto ToDto(Payment p)
        {
            return new PaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                UserId = p.UserId,
                OrderId = p.OrderId,
                Gateway = p.Gateway,
                GatewayReference = p.GatewayReference,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                Status = p.Status
            };
        }
    }
} 