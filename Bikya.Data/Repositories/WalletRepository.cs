using Bikya.Data;
using Bikya.Data.Enums;
using Bikya.Data.Models;
using Bikya.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Bikya.Data.Repositories
{
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        private readonly new BikyaContext _context;

        public WalletRepository(BikyaContext context, ILogger<GenericRepository<Wallet>> logger) : base(context, logger)
        {
            _context = context;
        }

        public async Task<Wallet?> GetWalletByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
        }

        public async Task<Wallet?> GetWalletWithTransactionsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Wallets
                .AsNoTracking()
                .Include(w => w.Transactions.OrderByDescending(t => t.CreatedAt))
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
        }

        public async Task<bool> WalletExistsForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Wallets
                .AsNoTracking()
                .AnyAsync(w => w.UserId == userId, cancellationToken);
        }

        public async Task<bool> IsWalletLockedAsync(int userId, CancellationToken cancellationToken = default)
        {
            var wallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            return wallet?.IsLocked ?? false;
        }

        public async Task<decimal> GetWalletBalanceAsync(int userId, CancellationToken cancellationToken = default)
        {
            var wallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            return wallet?.Balance ?? 0;
        }

        public async Task<bool> UpdateWalletBalanceAsync(int userId, decimal newBalance, CancellationToken cancellationToken = default)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wallet == null)
                return false;

            wallet.Balance = newBalance;
            _context.Wallets.Update(wallet);
            return true;
        }

        public async Task<bool> LockWalletAsync(int userId, CancellationToken cancellationToken = default)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wallet == null)
                return false;

            wallet.IsLocked = true;
            _context.Wallets.Update(wallet);
            return true;
        }

        public async Task<bool> UnlockWalletAsync(int userId, CancellationToken cancellationToken = default)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wallet == null)
                return false;

            wallet.IsLocked = false;
            _context.Wallets.Update(wallet);
            return true;
        }

        public async Task<bool> LinkPaymentMethodAsync(int userId, string methodName, CancellationToken cancellationToken = default)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wallet == null)
                return false;

            wallet.LinkedPaymentMethod = methodName;
            _context.Wallets.Update(wallet);
            return true;
        }

        public async Task<IEnumerable<Wallet>> GetWalletsByBalanceRangeAsync(decimal minBalance, decimal maxBalance, CancellationToken cancellationToken = default)
        {
            return await _context.Wallets
                .AsNoTracking()
                .Where(w => w.Balance >= minBalance && w.Balance <= maxBalance)
                .OrderByDescending(w => w.Balance)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Wallet>> GetLockedWalletsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Wallets
                .AsNoTracking()
                .Where(w => w.IsLocked)
                .OrderBy(w => w.UserId)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalWalletsBalanceAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Wallets
                .AsNoTracking()
                .SumAsync(w => w.Balance, cancellationToken);
        }

        public async Task<int> GetWalletsCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Wallets
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }

        public override async Task AddAsync(Wallet entity, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.Balance = 0;
            entity.IsLocked = false;
            await _context.Wallets.AddAsync(entity, cancellationToken);
        }

        public override void Update(Wallet entity)
        {
            _context.Wallets.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;

            // Preserve CreatedAt field during updates
            _context.Entry(entity).Property(e => e.CreatedAt).IsModified = false;
        }
    }
}