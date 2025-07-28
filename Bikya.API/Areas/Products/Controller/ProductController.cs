using Bikya.Data.Models;
using Bikya.Data.Response;
using Bikya.DTOs.ProductDTO;
using Bikya.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bikya.API.Areas.Products.Controller
{
    /// <summary>
    /// Controller for managing product operations.
    /// </summary>
    [Area("Products")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly ProductImageService _productImageService;
        private readonly IWebHostEnvironment _env;

        public ProductController(
            IWebHostEnvironment env,
            ProductService productService,
            ProductImageService productImageService)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _productImageService = productImageService ?? throw new ArgumentNullException(nameof(productImageService));
        }

        #region Helper Methods

        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
        }

        #endregion

        #region Admin Operations

        /// <summary>
        /// Gets all products with images (Admin only).
        /// </summary>
        /// <returns>List of all products</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProductsWithImages()
        {
            var products = await _productService.GetAllProductsWithImagesAsync();


            return Ok(ApiResponse<IEnumerable<GetProductDTO>>.SuccessResponse(products));
        }

        /// <summary>
        /// Gets all not approved products with images (Admin only).
        /// </summary>
        /// <returns>List of not approved products</returns>
        //[Authorize(Roles = "Admin")]
        //[HttpGet("not-approved")]
        //public async Task<IActionResult> GetNotApprovedProductsWithImages()
        //{
        //    var products = await _productService.GetNotApprovedProductsWithImagesAsync();
        //    return Ok(ApiResponse<IEnumerable<GetProductDTO>>.SuccessResponse(products));
        //}

        /// <summary>
        /// Approves a product (Admin only).
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Approval result</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveProduct(int id)
        {
            await _productService.ApproveProductAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(true));
        }

        /// <summary>
        /// Rejects a product (Admin only).
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Rejection result</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> RejectProduct(int id)
        {
            await _productService.RejectProductAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(true));
        }

        #endregion

        #region Public Operations

        /// <summary>
        /// Gets all approved products with images.
        /// </summary>
        /// <returns>List of approved products</returns>
        [HttpGet("approved")]
        public async Task<IActionResult> GetApprovedProductsWithImages()
        {
            var products = await _productService.GetApprovedProductsWithImagesAsync();
            return Ok(ApiResponse<IEnumerable<GetProductDTO>>.SuccessResponse(products));
        }

        /// <summary>
        /// Gets a product by ID with images.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        [HttpGet("Product/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductWithImagesByIdAsync(id);
            return Ok(ApiResponse<GetProductDTO>.SuccessResponse(product));
        }

        /// <summary>
        /// Gets products by user ID.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's products</returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetProductsByUser(int userId)
        {
            var products = await _productService.GetProductsByUserAsync(userId);
            return Ok(ApiResponse<IEnumerable<GetProductDTO>>.SuccessResponse(products));
        }

        /// <summary>
        /// Gets  approved products by user ID.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's not approved products</returns>
        /// 

        [HttpGet("approved/user/{userId}")]
        public async Task<IActionResult> GetApprovedProductsByUser(int userId)
        {
            var products = await _productService.GetApprovedProductsByUserAsync(userId);
            return Ok(ApiResponse<IEnumerable<GetProductDTO>>.SuccessResponse(products));
        }

        /// <summary>
        /// Gets products by category ID.
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>List of products in category</returns>
        [HttpGet("category/{id}")]
        public async Task<IActionResult> GetProductsByCategory(int id)
        {
            var products = await _productService.GetProductsByCategoryAsync(id);
            return Ok(ApiResponse<IEnumerable<GetProductDTO>>.SuccessResponse(products));
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="product">Product data</param>
        /// <returns>Creation result</returns>
        /// 

        //[Authorize]
        //[HttpPost]
        //public async Task<IActionResult> CreateProduct([FromBody] ProductDTO product)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    if (!TryGetUserId(out int userId))
        //        return Unauthorized(ApiResponse<string>.ErrorResponse("Unauthorized", 401));

        //    await _productService.CreateProductAsync(product, userId);
        //    return Ok(ApiResponse<bool>.SuccessResponse(true));
        //}

        /// <summary>
        /// Creates a new product with images.
        /// </summary>
        /// <param name="productDTO">Product data with images</param>
        /// <returns>Creation result</returns>
        [Authorize]
        [Consumes("multipart/form-data")]
        [HttpPost("add")]
        public async Task<IActionResult> CreateProductWithImages([FromForm] CreateProductWithimagesDTO productDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!TryGetUserId(out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Unauthorized", 401));

            var product = new ProductDTO
            {
                Title = productDTO.Title,
                Description = productDTO.Description,
                Price = productDTO.Price,
                IsForExchange = productDTO.IsForExchange,
                Condition = productDTO.Condition,
                CategoryId = productDTO.CategoryId
            };

            var createdProduct = await _productService.CreateProductAsync(product, userId);
            var rootPath = _env.WebRootPath;

            if (productDTO.MainImage != null)
            {
                await _productImageService.AddProductImageAsync(new ProductImageDTO
                {
                    ProductId = createdProduct.Id,
                    Image = productDTO.MainImage,
                    IsMain = true
                }, userId, rootPath);
            }

            if (productDTO.AdditionalImages?.Any() == true)
            {
                foreach (var image in productDTO.AdditionalImages)
                {
                    await _productImageService.AddProductImageAsync(new ProductImageDTO
                    {
                        ProductId = createdProduct.Id,
                        Image = image,
                        IsMain = false
                    }, userId, rootPath);
                }
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true));
        }

        /// <summary>
        /// Updates a product.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="product">Updated product data</param>
        /// <returns>Update result</returns>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDTO product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!TryGetUserId(out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Unauthorized", 401));

            await _productService.UpdateProductAsync(id, product, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(true));
        }

        /// <summary>
        /// Deletes a product.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Deletion result</returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Unauthorized", 401));

            var rootPath = _env.WebRootPath;
            await _productService.DeleteProductAsync(id, userId, rootPath);
            return Ok(ApiResponse<bool>.SuccessResponse(true));
        }

        #endregion

        #region Image Operations

        /// <summary>
        /// Adds an image to a product.
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="dto">Image data</param>
        /// <returns>Image addition result</returns>
        [Authorize]
        [HttpPost("{productId}/images")]
        public async Task<IActionResult> AddImage(int productId, [FromForm] CreateImageDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!TryGetUserId(out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Unauthorized", 401));

            var rootPath = _env.WebRootPath;

            if (dto.Image != null)
            {
                await _productImageService.AddProductImageAsync(new ProductImageDTO
                {
                    ProductId = productId,
                    Image = dto.Image,
                    IsMain = dto.IsMain
                }, userId, rootPath);
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true));
        }

        /// <summary>
        /// Deletes a product image.
        /// </summary>
        /// <param name="id">Image ID</param>
        /// <returns>Image deletion result</returns>
        //[Authorize]
        //[HttpPut("image/{id}")]
        //public async Task<IActionResult> updatetImage(int id,[FromForm] UpdatProductImage dto) 
        //    {
        //        if (!TryGetUserId(out int userId))
        //            return Unauthorized(ApiResponse<string>.ErrorResponse("Unauthorized", 401));

        //        var rootPath = _env.WebRootPath;
        //        if (dto.Image != null)
        //        {
        //            await _productImageService.UpdateProductImageAsync(id, userId, rootPath, dto.Image);
        //        }
        //        return Ok(ApiResponse<bool>.SuccessResponse(true));
        //    }


        [Authorize]
        [HttpPut("image/{id}/set-main")]
        public async Task<IActionResult> SetMainImage(int id)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Unauthorized", 401));

            await _productImageService.SetMainImageAsync(id, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Main image updated successfully"));
        }

        /// <summary>
        /// Deletes a product image.
        /// </summary>
        /// <param name="id">Image ID</param>
        /// <returns>Image deletion result</returns>
        [Authorize]
            [HttpDelete("image/{id}")]
            public async Task<IActionResult> DeleteProductImage(int id)
            {
                if (!TryGetUserId(out int userId))
                    return Unauthorized(ApiResponse<string>.ErrorResponse("Unauthorized", 401));

                var rootPath = _env.WebRootPath;
                await _productImageService.DeleteProductImageAsync(id, userId, rootPath);
                return Ok(ApiResponse<bool>.SuccessResponse(true));
            }

            #endregion
        }
    } 

