using Bikya.Data.Models;

namespace Bikya.Data.Repositories.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<(IEnumerable<Category> categories, int totalCount)> GetPaginatedAsync(
            int page,
            int pageSize,
            string? search = null,
            CancellationToken cancellationToken = default);

        Task<Category?> GetByIdWithProductsAsync(int id, CancellationToken cancellationToken = default);

        Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<Category?> GetByNameWithProductsAsync(string name, CancellationToken cancellationToken = default);

        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<bool> ExistsByNameExcludingIdAsync(string name, int excludeId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Category>> GetOrderedByCreatedDateAsync(CancellationToken cancellationToken = default);
    }
}