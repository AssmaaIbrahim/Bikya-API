using Bikya.Data.Enums;
using Bikya.Data.Models;
using Bikya.Data.Repositories.Interfaces;
using Bikya.Data.Response;
using Bikya.DTOs.WalletDTOs;
using Bikya.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bikya.Services.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;

        public WalletService(IWalletRepository walletRepository, ITransactionRepository transactionRepository)
        {
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<ApiResponse<bool>> ConfirmTransactionAsync(int transactionId)
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId);

            if (transaction == null)
                return ApiResponse<bool>.ErrorResponse("Transaction not found", 404);

            if (transaction.Status == TransactionStatus.Completed)
                return ApiResponse<bool>.ErrorResponse("Transaction already completed", 400);

            if (transaction.Status == TransactionStatus.Cancelled || transaction.Status == TransactionStatus.Failed)
                return ApiResponse<bool>.ErrorResponse("Cannot confirm a failed or cancelled transaction", 400);

            var success = await _transactionRepository.UpdateTransactionStatusAsync(transactionId, TransactionStatus.Completed);
            if (!success)
                return ApiResponse<bool>.ErrorResponse("Failed to update transaction status", 500);

            await _transactionRepository.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Transaction confirmed successfully");
        }

        public async Task<ApiResponse<WalletDto>> CreateWalletAsync(int userId)
        {
            var existing = await _walletRepository.WalletExistsForUserAsync(userId);
            if (existing)
                return ApiResponse<WalletDto>.ErrorResponse("Wallet already exists", 400);

            var wallet = new Wallet
            {
                UserId = userId
            };

            await _walletRepository.AddAsync(wallet);
            await _walletRepository.SaveChangesAsync();

            var walletDto = new WalletDto
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Balance = wallet.Balance,
                LinkedPaymentMethod = wallet.LinkedPaymentMethod,
                CreatedAt = wallet.CreatedAt
            };

            return ApiResponse<WalletDto>.SuccessResponse(walletDto, "Wallet created", 201);
        }

        public async Task<ApiResponse<WalletDto>> DepositAsync(int userId, decimal amount, string? description)
        {
            try
            {
                if (amount <= 0)
                    return ApiResponse<WalletDto>.ErrorResponse("Amount must be greater than zero", 400);

                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                    return ApiResponse<WalletDto>.ErrorResponse("Wallet not found", 404);

                var newBalance = wallet.Balance + amount;
                var success = await _walletRepository.UpdateWalletBalanceAsync(userId, newBalance);
                if (!success)
                    return ApiResponse<WalletDto>.ErrorResponse("Failed to update wallet balance", 500);

                var transaction = new Transaction
                {
                    Amount = amount,
                    Type = TransactionType.Deposit,
                    WalletId = wallet.Id,
                    Description = description,
                    Status = TransactionStatus.Completed
                };

                await _transactionRepository.AddAsync(transaction);
                await _walletRepository.SaveChangesAsync();

                var dto = new WalletDto
                {
                    Id = wallet.Id,
                    Balance = newBalance,
                    UserId = wallet.UserId,
                    LinkedPaymentMethod = wallet.LinkedPaymentMethod,
                    CreatedAt = wallet.CreatedAt
                };

                return ApiResponse<WalletDto>.SuccessResponse(dto, "Deposit successful");
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "";
                return ApiResponse<WalletDto>.ErrorResponse("Technical error: " + ex.Message + (string.IsNullOrEmpty(inner) ? "" : $" | Inner: {inner}"), 500);
            }
        }

        public async Task<ApiResponse<decimal>> GetBalanceAsync(int userId)
        {
            var balance = await _walletRepository.GetWalletBalanceAsync(userId);
            if (balance == 0 && !await _walletRepository.WalletExistsForUserAsync(userId))
                return ApiResponse<decimal>.ErrorResponse("Wallet not found", 404);

            return ApiResponse<decimal>.SuccessResponse(balance);
        }

        public async Task<ApiResponse<TransactionDto>> GetTransactionByIdAsync(int userId, int id)
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
                return ApiResponse<TransactionDto>.ErrorResponse("Wallet not found", 404);

            var transaction = await _transactionRepository.GetTransactionByIdAndWalletAsync(id, wallet.Id);
            if (transaction == null)
                return ApiResponse<TransactionDto>.ErrorResponse("Transaction not found", 404);

            var transactionDto = new TransactionDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Type = transaction.Type.ToString(),
                Description = transaction.Description,
                Status = transaction.Status.ToString(),
                CreatedAt = transaction.CreatedAt
            };

            return ApiResponse<TransactionDto>.SuccessResponse(transactionDto);
        }

        public async Task<ApiResponse<List<TransactionDto>>> GetTransactionsAsync(int userId)
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
                return ApiResponse<List<TransactionDto>>.ErrorResponse("Wallet not found", 404);

            var transactions = await _transactionRepository.GetTransactionsByWalletIdAsync(wallet.Id);

            var transactionDtos = transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                Status = t.Status.ToString()
            }).ToList();

            return ApiResponse<List<TransactionDto>>.SuccessResponse(transactionDtos);
        }

        public async Task<ApiResponse<bool>> LinkPaymentMethodAsync(int userId, string methodName)
        {
            var success = await _walletRepository.LinkPaymentMethodAsync(userId, methodName);
            if (!success)
                return ApiResponse<bool>.ErrorResponse("Wallet not found", 404);

            await _walletRepository.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, $"Payment method '{methodName}' linked successfully");
        }

        public async Task<ApiResponse<bool>> LockWalletAsync(int userId)
        {
            var isLocked = await _walletRepository.IsWalletLockedAsync(userId);
            if (isLocked)
                return ApiResponse<bool>.ErrorResponse("Wallet is already locked", 400);

            var success = await _walletRepository.LockWalletAsync(userId);
            if (!success)
                return ApiResponse<bool>.ErrorResponse("Wallet not found", 404);

            await _walletRepository.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Wallet locked successfully");
        }

        public async Task<ApiResponse<WalletDto>> PayAsync(int userId, decimal amount, int orderId, string? description)
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
                return ApiResponse<WalletDto>.ErrorResponse("Wallet not found", 404);

            if (wallet.IsLocked)
                return ApiResponse<WalletDto>.ErrorResponse("Wallet is locked", 403);

            if (wallet.Balance < amount)
                return ApiResponse<WalletDto>.ErrorResponse("Insufficient balance", 400);

            var newBalance = wallet.Balance - amount;
            var success = await _walletRepository.UpdateWalletBalanceAsync(userId, newBalance);
            if (!success)
                return ApiResponse<WalletDto>.ErrorResponse("Failed to update wallet balance", 500);

            var transaction = new Transaction
            {
                WalletId = wallet.Id,
                Amount = amount,
                Type = TransactionType.Payment,
                Description = $"Payment for Order #{orderId} - {description}",
                Status = TransactionStatus.Completed,
                RelatedOrderId = orderId
            };

            await _transactionRepository.AddAsync(transaction);
            await _walletRepository.SaveChangesAsync();

            var walletDto = new WalletDto
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Balance = newBalance,
                CreatedAt = wallet.CreatedAt,
                LinkedPaymentMethod = wallet.LinkedPaymentMethod
            };

            return ApiResponse<WalletDto>.SuccessResponse(walletDto, "Payment successful");
        }

        public async Task<ApiResponse<WalletDto>> RefundAsync(int userId, int transactionId, string reason)
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
                return ApiResponse<WalletDto>.ErrorResponse("Wallet not found", 404);

            var originalTransaction = await _transactionRepository.GetTransactionByIdAndWalletAsync(transactionId, wallet.Id);
            if (originalTransaction == null)
                return ApiResponse<WalletDto>.ErrorResponse("Original transaction not found", 404);

            if (originalTransaction.Type != TransactionType.Payment)
                return ApiResponse<WalletDto>.ErrorResponse("Only payments can be refunded", 400);

            if (originalTransaction.Status != TransactionStatus.Completed)
                return ApiResponse<WalletDto>.ErrorResponse("Transaction is not completed", 400);

            var newBalance = wallet.Balance + originalTransaction.Amount;
            var success = await _walletRepository.UpdateWalletBalanceAsync(userId, newBalance);
            if (!success)
                return ApiResponse<WalletDto>.ErrorResponse("Failed to update wallet balance", 500);

            var refundTransaction = new Transaction
            {
                WalletId = wallet.Id,
                Amount = originalTransaction.Amount,
                Type = TransactionType.Refund,
                Description = $"Refund for Transaction #{transactionId} - {reason}",
                Status = TransactionStatus.Completed
            };

            await _transactionRepository.AddAsync(refundTransaction);
            await _walletRepository.SaveChangesAsync();

            var walletDto = new WalletDto
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Balance = newBalance,
                CreatedAt = wallet.CreatedAt,
                LinkedPaymentMethod = wallet.LinkedPaymentMethod
            };

            return ApiResponse<WalletDto>.SuccessResponse(walletDto, "Refund completed successfully");
        }

        public async Task<ApiResponse<WalletDto>> WithdrawAsync(int userId, decimal amount, string? description)
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
                return ApiResponse<WalletDto>.ErrorResponse("Wallet not found", 404);

            if (wallet.Balance < amount)
                return ApiResponse<WalletDto>.ErrorResponse("Insufficient balance", 400);

            var newBalance = wallet.Balance - amount;
            var success = await _walletRepository.UpdateWalletBalanceAsync(userId, newBalance);
            if (!success)
                return ApiResponse<WalletDto>.ErrorResponse("Failed to update wallet balance", 500);

            var transaction = new Transaction
            {
                Amount = amount,
                Type = TransactionType.Withdraw,
                WalletId = wallet.Id,
                Description = description,
                Status = TransactionStatus.Completed
            };

            await _transactionRepository.AddAsync(transaction);
            await _walletRepository.SaveChangesAsync();

            var walletDto = new WalletDto
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Balance = newBalance,
                CreatedAt = wallet.CreatedAt,
                LinkedPaymentMethod = wallet.LinkedPaymentMethod
            };

            return ApiResponse<WalletDto>.SuccessResponse(walletDto, "Withdraw successful");
        }
    }
}