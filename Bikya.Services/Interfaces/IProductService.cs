using Bikya.Data.Models;
using Bikya.DTOs.ProductDTO;

namespace Bikya.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing product business logic.
    /// </summary>
    public interface IProductService
    {
        #region User Validation

        /// <summary>
        /// Checks if a user exists by ID.
        /// </summary>
        Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a user has admin role.
        /// </summary>
        Task<bool> IsAdminAsync(int userId, CancellationToken cancellationToken = default);

        #endregion

        #region GET Methods

        /// <summary>
        /// Gets all approved products with images.
        /// </summary>
        Task<IEnumerable<Product>> GetApprovedProductsWithImagesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all not approved products with images.
        /// </summary>
        Task<IEnumerable<Product>> GetNotApprovedProductsWithImagesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all products with images.
        /// </summary>
        Task<IEnumerable<Product>> GetAllProductsWithImagesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a product by ID with images.
        /// </summary>
        Task<Product> GetProductWithImagesByIdAsync(int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all products for a specific user.
        /// </summary>
        Task<IEnumerable<Product>> GetProductsByUserAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all not approved products for a specific user.
        /// </summary>
        Task<IEnumerable<Product>> GetNotApprovedProductsByUserAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all products for a specific category.
        /// </summary>
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Creates a new product.
        /// </summary>
        Task<Product> CreateProductAsync(ProductDTO productDTO, int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        Task UpdateProductAsync(int id, ProductDTO productDTO, int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a product and its associated images.
        /// </summary>
        Task DeleteProductAsync(int id, int userId, string rootPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Approves a product.
        /// </summary>
        Task ApproveProductAsync(int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rejects a product.
        /// </summary>
        Task RejectProductAsync(int productId, CancellationToken cancellationToken = default);

        #endregion
    }
} 