using System.Threading.Tasks;
using Bikya.DTOs.PaymentDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bikya.API.Areas.Wallet.Controllers
{
    [ApiController]
    [Route("api/[area]/[controller]")]
    [Area("Wallet")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        // رسائل الأخطاء الموحدة
        private const string InvalidUserIdMessage = "Invalid user ID";
        private const string InvalidPaymentIdMessage = "Invalid payment ID";

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Creates a new payment.
        /// </summary>
        /// <param name="dto">Payment creation data</param>
        /// <returns>Payment creation result</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _paymentService.CreatePaymentAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Gets all payments for a user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of payments</returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            if (userId <= 0)
                return BadRequest(new { message = InvalidUserIdMessage });

            var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
            return Ok(payments);
        }

        /// <summary>
        /// Gets a payment by its ID.
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>Payment details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = InvalidPaymentIdMessage });

            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null) return NotFound(new { message = "Payment not found" });
            return Ok(payment);
        }
    }
} 