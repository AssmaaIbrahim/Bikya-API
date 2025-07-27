
using Bikya.Data.Models;
using Bikya.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bikya.Data.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private new readonly BikyaContext _context;

        public CategoryRepository(BikyaContext context, ILogger<CategoryRepository> logger)
            : base(context, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(IEnumerable<Category> categories, int totalCount)> GetPaginatedAsync(
            int page,
            int pageSize,
            string? search = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _context.Categories.AsNoTracking().AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(c => c.Name.Contains(search));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var categories = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return (categories, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated categories");
                throw;
            }
        }

        public async Task<Category?> GetByIdWithProductsAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Categories
                    .AsNoTracking()
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID {CategoryId} and products", id);
                throw;
            }
        }

        public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category by name {CategoryName}", name);
                throw;
            }
        }

        public async Task<Category?> GetByNameWithProductsAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Categories
                    .AsNoTracking()
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category by name {CategoryName} with products", name);
                throw;
            }
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Categories
                    .AsNoTracking()
                    .AnyAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if category exists by name {CategoryName}", name);
                throw;
            }
        }

        public async Task<bool> ExistsByNameExcludingIdAsync(string name, int excludeId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Categories
                    .AsNoTracking()
                    .AnyAsync(c => c.Id != excludeId && c.Name.ToLower() == name.ToLower(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if category exists by name {CategoryName} excluding ID {ExcludeId}", name, excludeId);
                throw;
            }
        }

        public async Task<IEnumerable<Category>> GetOrderedByCreatedDateAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Categories
                    .AsNoTracking()
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories ordered by created date");
                throw;
            }
        }

        public override async Task<Category?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == (int)id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID {CategoryId}", id);
                throw;
            }
        }

        public override async Task AddAsync(Category entity, CancellationToken cancellationToken = default)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                await base.AddAsync(entity, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding category {CategoryName}", entity.Name);
                throw;
            }
        }

        public override void Update(Category entity)
        {
            try
            {
                base.Update(entity);

                // Preserve CreatedAt field during updates
                _context.Entry(entity).Property(e => e.CreatedAt).IsModified = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", entity.Id);
                throw;
            }
        }

        public async Task AddRangeAsync(List<Category> categories)
        {
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Category>> GetAllAsync(string? search = null)
        {
            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search));
            }

            return await query.ToListAsync();
        }

    }
}