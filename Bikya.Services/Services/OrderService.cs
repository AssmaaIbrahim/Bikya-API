using Bikya.Data.Repositories.Interfaces;
using Bikya.Data.Enums;
using Bikya.Data.Models;
using Bikya.Data.Response;
using Bikya.DTOs.Orderdto;
using Bikya.DTOs.ShippingDTOs;
using Bikya.Services.Interfaces;

namespace Bikya.Services.Services
{
    public class OrderService : IOrderService
    {
        // رسائل الأخطاء والنجاح الموحدة
        private const string ProductNotFoundMessage = "Product not found";
        private const string SellerNotFoundMessage = "Seller not found";
        private const string OrderNotFoundMessage = "Order not found";
        private const string InvalidStatusMessage = "Invalid status";
        private const string NotAuthorizedMessage = "Not authorized or order cannot be canceled";
        private const string FailedCancelOrderMessage = "Failed to cancel order";
        private const string OrderOrShippingNotFoundMessage = "Order or shipping info not found";
        private const string OrderCreatedSuccessMessage = "Order created successfully";
        private const string OrderStatusUpdatedSuccessMessage = "Order status updated successfully";
        private const string OrderCanceledSuccessMessage = "Order canceled successfully";
        private const string ShippingInfoUpdatedSuccessMessage = "Shipping info updated successfully";

        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IShippingServiceRepository _shippingInfoRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IShippingServiceRepository shippingInfoRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _shippingInfoRepository = shippingInfoRepository;
        }

        /// <summary>
        /// Creates a new order for a product and buyer.
        /// </summary>
        /// <param name="dto">Order creation data</param>
        /// <returns>OrderDTO with details or error</returns>
        public async Task<ApiResponse<OrderDTO>> CreateOrderAsync(CreateOrderDTO dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null)
                return ApiResponse<OrderDTO>.ErrorResponse(ProductNotFoundMessage, 404);

            var seller = await _userRepository.FindByIdAsync(product.UserId.Value);
            if (seller == null)
                return ApiResponse<OrderDTO>.ErrorResponse(SellerNotFoundMessage, 404);

            var order = new Order
            {
                ProductId = dto.ProductId,
                BuyerId = dto.BuyerId,
                SellerId = seller.Id,
                TotalAmount = product.Price,
                PlatformFee = product.Price * 0.05m,
                SellerAmount = product.Price * 0.95m,
                ShippingInfo = new ShippingInfo
                {
                    RecipientName = dto.ShippingInfo.RecipientName,
                    Address = dto.ShippingInfo.Address,
                    City = dto.ShippingInfo.City,
                    PostalCode = dto.ShippingInfo.PostalCode,
                    PhoneNumber = dto.ShippingInfo.PhoneNumber,
                    Status = ShippingStatus.Pending,
                }
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            return ApiResponse<OrderDTO>.SuccessResponse(new OrderDTO
            {
                Id = order.Id,
                ProductId = product.Id,
                ProductTitle = product.Title,
                BuyerId = dto.BuyerId,
                BuyerName = "", // Can be populated if needed
                SellerId = seller.Id,
                SellerName = seller.FullName,
                TotalAmount = order.TotalAmount,
                PlatformFee = order.PlatformFee,
                SellerAmount = order.SellerAmount,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                ShippingInfo = dto.ShippingInfo
            }, OrderCreatedSuccessMessage);
        }

        /// <summary>
        /// Updates the status of an order. (يفضل التحقق من صلاحية المستخدم في الكنترولر)
        /// </summary>
        /// <param name="dto">Order status update data</param>
        /// <returns>ApiResponse indicating success or error</returns>
        public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(UpdateOrderStatusDTO dto)
        {
            if (!Enum.TryParse<OrderStatus>(dto.NewStatus, true, out var newStatus))
                return ApiResponse<bool>.ErrorResponse(InvalidStatusMessage, 400);

            // مثال: تحقق من صلاحية المستخدم (يمكن نقلها للكنترولر)
            // if (!UserIsAuthorizedToUpdateStatus(dto.OrderId, dto.UserId))
            //     return ApiResponse<bool>.ErrorResponse(NotAuthorizedMessage, 403);

            var success = await _orderRepository.UpdateOrderStatusAsync(dto.OrderId, newStatus);
            if (!success)
                return ApiResponse<bool>.ErrorResponse(OrderNotFoundMessage, 404);

            await _orderRepository.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, OrderStatusUpdatedSuccessMessage);
        }

        /// <summary>
        /// Gets all orders for a user (buyer or seller).
        /// </summary>
        public async Task<ApiResponse<List<OrderSummaryDTO>>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);

            var orderSummaries = orders.Select(o => new OrderSummaryDTO
            {
                Id = o.Id,
                ProductTitle = o.Product?.Title ?? string.Empty,
                BuyerName = o.Buyer?.UserName ?? string.Empty,
                SellerName = o.Seller?.UserName ?? string.Empty,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            }).ToList();

            return ApiResponse<List<OrderSummaryDTO>>.SuccessResponse(orderSummaries);
        }

        /// <summary>
        /// Gets all orders where the user is the buyer.
        /// </summary>
        public async Task<ApiResponse<List<OrderSummaryDTO>>> GetOrdersByBuyerIdAsync(int buyerId)
        {
            var orders = await _orderRepository.GetOrdersByBuyerIdAsync(buyerId);

            var orderSummaries = orders.Select(o => new OrderSummaryDTO
            {
                Id = o.Id,
                ProductTitle = o.Product?.Title ?? string.Empty,
                BuyerName = o.Buyer?.UserName ?? string.Empty,
                SellerName = o.Seller?.UserName ?? string.Empty,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            }).ToList();

            return ApiResponse<List<OrderSummaryDTO>>.SuccessResponse(orderSummaries);
        }

        /// <summary>
        /// Gets all orders where the user is the seller.
        /// </summary>
        public async Task<ApiResponse<List<OrderSummaryDTO>>> GetOrdersBySellerIdAsync(int sellerId)
        {
            var orders = await _orderRepository.GetOrdersBySellerIdAsync(sellerId);

            var orderSummaries = orders.Select(o => new OrderSummaryDTO
            {
                Id = o.Id,
                ProductTitle = o.Product?.Title ?? string.Empty,
                BuyerName = o.Buyer?.UserName ?? string.Empty,
                SellerName = o.Seller?.UserName ?? string.Empty,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            }).ToList();

            return ApiResponse<List<OrderSummaryDTO>>.SuccessResponse(orderSummaries);
        }

        /// <summary>
        /// Gets all orders (admin only).
        /// </summary>
        public async Task<ApiResponse<List<OrderSummaryDTO>>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersWithRelationsAsync();

            var orderSummaries = orders.Select(o => new OrderSummaryDTO
            {
                Id = o.Id,
                ProductTitle = o.Product?.Title ?? string.Empty,
                BuyerName = o.Buyer?.UserName ?? string.Empty,
                SellerName = o.Seller?.UserName ?? string.Empty,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            }).ToList();

            return ApiResponse<List<OrderSummaryDTO>>.SuccessResponse(orderSummaries);
        }

        /// <summary>
        /// Gets order details by order ID.
        /// </summary>
        public async Task<ApiResponse<OrderDetailsDTO>> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderWithAllRelationsAsync(orderId);

            if (order == null)
                return ApiResponse<OrderDetailsDTO>.ErrorResponse(OrderNotFoundMessage, 404);

            var orderDetails = new OrderDetailsDTO
            {
                Id = order.Id,
                ProductId = order.ProductId,
                ProductTitle = order.Product?.Title ?? string.Empty,
                BuyerId = order.BuyerId,
                BuyerName = order.Buyer?.UserName ?? string.Empty,
                SellerId = order.SellerId,
                SellerName = order.Seller?.UserName ?? string.Empty,
                TotalAmount = order.TotalAmount,
                PlatformFee = order.PlatformFee,
                SellerAmount = order.SellerAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                CompletedAt = order.CompletedAt,
                ShippingInfo = order.ShippingInfo == null ? null : new ShippingDetailsDto
                {
                    ShippingId = order.ShippingInfo.ShippingId,
                    RecipientName = order.ShippingInfo.RecipientName,
                    Address = order.ShippingInfo.Address,
                    City = order.ShippingInfo.City,
                    PostalCode = order.ShippingInfo.PostalCode,
                    PhoneNumber = order.ShippingInfo.PhoneNumber,
                    Status = order.ShippingInfo.Status,
                    CreateAt = order.ShippingInfo.CreateAt,
                    OrderId = order.ShippingInfo.OrderId
                }
            };

            return ApiResponse<OrderDetailsDTO>.SuccessResponse(orderDetails);
        }

        /// <summary>
        /// Cancels an order if the buyer is authorized.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>ApiResponse indicating success or error</returns>
        public async Task<ApiResponse<bool>> CancelOrderAsync(int orderId, int buyerId)
        {
            var canCancel = await _orderRepository.CanUserCancelOrderAsync(orderId, buyerId);
            if (!canCancel)
                return ApiResponse<bool>.ErrorResponse(NotAuthorizedMessage, 403);

            var success = await _orderRepository.UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled);
            if (!success)
                return ApiResponse<bool>.ErrorResponse(FailedCancelOrderMessage, 500);

            await _orderRepository.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, OrderCanceledSuccessMessage);
        }

        /// <summary>
        /// Updates the shipping information for an order.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="dto">Shipping info data</param>
        /// <returns>ApiResponse indicating success or error</returns>
        public async Task<ApiResponse<bool>> UpdateShippingInfoAsync(int orderId, ShippingInfoDTO dto)
        {
            var order = await _orderRepository.GetOrderWithShippingInfoAsync(orderId);

            if (order == null || order.ShippingInfo == null)
                return ApiResponse<bool>.ErrorResponse(OrderOrShippingNotFoundMessage, 404);

            order.ShippingInfo.RecipientName = dto.RecipientName;
            order.ShippingInfo.Address = dto.Address;
            order.ShippingInfo.City = dto.City;
            order.ShippingInfo.PostalCode = dto.PostalCode;
            order.ShippingInfo.PhoneNumber = dto.PhoneNumber;

            _shippingInfoRepository.Update(order.ShippingInfo);
            await _shippingInfoRepository.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, ShippingInfoUpdatedSuccessMessage);
        }
    }
}