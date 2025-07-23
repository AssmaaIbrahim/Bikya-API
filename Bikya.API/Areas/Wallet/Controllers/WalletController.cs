using Bikya.DTOs.WalletDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bikya.API.Areas.Wallet.Controllers
{
    /// <summary>
    /// Controller for managing wallet operations.
    /// </summary>
    [Area("Wallet")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        // رسائل الأخطاء الموحدة
        private const string InvalidUserIdMessage = "Invalid user ID";
        private const string InvalidTransactionIdMessage = "Invalid transaction ID";

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        }

        /// <summary>
        /// Creates a new wallet for a user.
        /// </summary>
        /// <param name="dto">User ID request</param>
        /// <returns>Wallet creation result</returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateWallet([FromBody] UserIdRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _walletService.CreateWalletAsync(dto.UserId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Gets the balance of a user's wallet.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Wallet balance</returns>
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance([FromQuery] int userId)
        {
            if (userId <= 0)
                return BadRequest(new { message = InvalidUserIdMessage });

            var result = await _walletService.GetBalanceAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Deposits money into a user's wallet.
        /// </summary>
        /// <param name="dto">Deposit request data</param>
        /// <returns>Deposit result</returns>
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _walletService.DepositAsync(dto.UserId, dto.Amount, dto.Description);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Withdraws money from a user's wallet.
        /// </summary>
        /// <param name="dto">Withdraw request data</param>
        /// <returns>Withdraw result</returns>
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _walletService.WithdrawAsync(dto.UserId, dto.Amount, dto.Description);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Processes a payment from a user's wallet.
        /// </summary>
        /// <param name="dto">Payment request data</param>
        /// <returns>Payment result</returns>
        [HttpPost("pay")]
        public async Task<IActionResult> Pay([FromBody] PayRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _walletService.PayAsync(dto.UserId, dto.Amount, dto.OrderId, dto.Description);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Processes a refund to a user's wallet.
        /// </summary>
        /// <param name="dto">Refund request data</param>
        /// <returns>Refund result</returns>
        [HttpPost("refund")]
        public async Task<IActionResult> Refund([FromBody] RefundRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _walletService.RefundAsync(dto.UserId, dto.TransactionId, dto.Reason);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Gets all transactions for a user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of transactions</returns>
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] int userId)
        {
            if (userId <= 0)
                return BadRequest(new { message = InvalidUserIdMessage });

            var result = await _walletService.GetTransactionsAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Gets a specific transaction by ID.
        /// </summary>
        /// <param name="id">Transaction ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>Transaction details</returns>
        [HttpGet("transaction/{id}")]
        public async Task<IActionResult> GetTransactionById(int id, [FromQuery] int userId)
        {
            if (id <= 0)
                return BadRequest(new { message = InvalidTransactionIdMessage });

            if (userId <= 0)
                return BadRequest(new { message = InvalidUserIdMessage });

            var result = await _walletService.GetTransactionByIdAsync(userId, id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Locks a user's wallet.
        /// </summary>
        /// <param name="dto">User ID request</param>
        /// <returns>Lock result</returns>
        [HttpPost("lock")]
        public async Task<IActionResult> LockWallet([FromBody] UserIdRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _walletService.LockWalletAsync(dto.UserId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Confirms a transaction.
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <returns>Confirmation result</returns>
        [HttpPost("confirm/{transactionId}")]
        public async Task<IActionResult> ConfirmTransaction(int transactionId)
        {
            if (transactionId <= 0)
                return BadRequest(new { message = InvalidTransactionIdMessage });

            var result = await _walletService.ConfirmTransactionAsync(transactionId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Links a payment method to a user's wallet.
        /// </summary>
        /// <param name="dto">Payment method link data</param>
        /// <returns>Link result</returns>
        [HttpPost("link-method")]
        public async Task<IActionResult> LinkPaymentMethod([FromBody] LinkPaymentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _walletService.LinkPaymentMethodAsync(dto.UserId, dto.MethodName);
            return StatusCode(result.StatusCode, result);
        }
    }
}
