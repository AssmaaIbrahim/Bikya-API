using Bikya.Data.Enums;
using Bikya.Data.Models;
using Bikya.Data.Repositories.Interfaces;
using System.Linq.Expressions;

namespace Bikya.Data.Repositories.Interfaces
{
    public interface IWalletRepository : IGenericRepository<Wallet>
    {
        Task<Wallet?> GetWalletByUserIdAsync(int userId, CancellationToken cancellationToken = default);

        Task<Wallet?> GetWalletWithTransactionsAsync(int userId, CancellationToken cancellationToken = default);

        Task<bool> WalletExistsForUserAsync(int userId, CancellationToken cancellationToken = default);

        Task<bool> IsWalletLockedAsync(int userId, CancellationToken cancellationToken = default);

        Task<decimal> GetWalletBalanceAsync(int userId, CancellationToken cancellationToken = default);

        Task<bool> UpdateWalletBalanceAsync(int userId, decimal newBalance, CancellationToken cancellationToken = default);

        Task<bool> LockWalletAsync(int userId, CancellationToken cancellationToken = default);

        Task<bool> UnlockWalletAsync(int userId, CancellationToken cancellationToken = default);

        Task<bool> LinkPaymentMethodAsync(int userId, string methodName, CancellationToken cancellationToken = default);

        Task<IEnumerable<Wallet>> GetWalletsByBalanceRangeAsync(decimal minBalance, decimal maxBalance, CancellationToken cancellationToken = default);

        Task<IEnumerable<Wallet>> GetLockedWalletsAsync(CancellationToken cancellationToken = default);

        Task<decimal> GetTotalWalletsBalanceAsync(CancellationToken cancellationToken = default);

        Task<int> GetWalletsCountAsync(CancellationToken cancellationToken = default);
    }
}