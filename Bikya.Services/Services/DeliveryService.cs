using Bikya.Data.Enums;
using Bikya.Data.Models;
using Bikya.Data.Repositories.Interfaces;
using Bikya.Data.Response;
using Bikya.DTOs.AuthDTOs;
using Bikya.DTOs.DeliveryDTOs;
using Bikya.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bikya.Services.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtService _jwtService;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<DeliveryService> _logger;

        public DeliveryService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IJwtService jwtService,
            IOrderRepository orderRepository,
            ILogger<DeliveryService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(DeliveryLoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password", 401);
                }

                var isValidPassword = await _userManager.CheckPasswordAsync(user, loginDto.Password);
                if (!isValidPassword)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password", 401);
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains("Delivery"))
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Access denied. Delivery role required.", 403);
                }

                var token = await _jwtService.GenerateAccessTokenAsync(user);

                var response = new AuthResponseDto
                {
                    Token = token,
                    Email = user.Email ?? "",
                    FullName = user.FullName ?? "",
                    UserName = user.UserName ?? "",
                    UserId = user.Id,
                    Roles = userRoles.ToList()
                };

                return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for email: {Email}", loginDto.Email);
                return ApiResponse<AuthResponseDto>.ErrorResponse($"Login failed: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<List<DeliveryOrderDto>>> GetOrdersForDeliveryAsync()
        {
            try
            {
                var orders = await _orderRepository.GetAllOrdersWithRelationsAsync();
                
                var filteredOrders = orders.Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Shipped).ToList();

                var deliveryOrders = new List<DeliveryOrderDto>();

                foreach (var order in filteredOrders)
                {
                    var deliveryOrder = new DeliveryOrderDto
                    {
                        Id = order.Id,
                        ProductName = order.Product?.Title ?? "Unknown Product",
                        ProductImage = order.Product?.Images?.FirstOrDefault()?.ImageUrl ?? "",
                        TotalAmount = order.TotalAmount,
                        Status = order.Status,
                        CreatedAt = order.CreatedAt,
                        PaidAt = order.PaidAt,
                        RecipientName = order.ShippingInfo?.RecipientName ?? "",
                        Address = order.ShippingInfo?.Address ?? "",
                        City = order.ShippingInfo?.City ?? "",
                        PostalCode = order.ShippingInfo?.PostalCode ?? "",
                        PhoneNumber = order.ShippingInfo?.PhoneNumber ?? "",
                        ShippingStatus = order.ShippingInfo?.Status ?? ShippingStatus.Pending,
                        BuyerName = order.Buyer?.FullName ?? "",
                        BuyerEmail = order.Buyer?.Email ?? "",
                        BuyerPhone = order.Buyer?.PhoneNumber ?? ""
                    };
                    
                    deliveryOrders.Add(deliveryOrder);
                }

                return ApiResponse<List<DeliveryOrderDto>>.SuccessResponse(deliveryOrders, "Orders retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve orders for delivery");
                return ApiResponse<List<DeliveryOrderDto>>.ErrorResponse($"Failed to retrieve orders: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<DeliveryOrderDto>> GetOrderByIdAsync(int orderId)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithAllRelationsAsync(orderId);

                if (order == null)
                {
                    return ApiResponse<DeliveryOrderDto>.ErrorResponse("Order not found", 404);
                }

                var deliveryOrder = new DeliveryOrderDto
                {
                    Id = order.Id,
                    ProductName = order.Product?.Title ?? "Unknown Product",
                    ProductImage = order.Product?.Images?.FirstOrDefault()?.ImageUrl ?? "",
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    CreatedAt = order.CreatedAt,
                    PaidAt = order.PaidAt,
                    RecipientName = order.ShippingInfo?.RecipientName ?? "",
                    Address = order.ShippingInfo?.Address ?? "",
                    City = order.ShippingInfo?.City ?? "",
                    PostalCode = order.ShippingInfo?.PostalCode ?? "",
                    PhoneNumber = order.ShippingInfo?.PhoneNumber ?? "",
                    ShippingStatus = order.ShippingInfo?.Status ?? ShippingStatus.Pending,
                    BuyerName = order.Buyer?.FullName ?? "",
                    BuyerEmail = order.Buyer?.Email ?? "",
                    BuyerPhone = order.Buyer?.PhoneNumber ?? ""
                };

                return ApiResponse<DeliveryOrderDto>.SuccessResponse(deliveryOrder, "Order retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve order {OrderId}", orderId);
                return ApiResponse<DeliveryOrderDto>.ErrorResponse($"Failed to retrieve order: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<object>> GetOrderStatusSummaryAsync(int orderId)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithAllRelationsAsync(orderId);

                if (order == null)
                {
                    return ApiResponse<object>.ErrorResponse("Order not found", 404);
                }

                var statusSummary = new
                {
                    OrderId = order.Id,
                    OrderStatus = order.Status,
                    ShippingStatus = order.ShippingInfo?.Status ?? ShippingStatus.Pending,
                    IsSynchronized = IsStatusSynchronized(order.Status, order.ShippingInfo?.Status),
                    LastUpdated = order.CompletedAt ?? order.PaidAt ?? order.CreatedAt,
                    NextAllowedTransitions = GetNextAllowedTransitions(order.Status, order.ShippingInfo?.Status)
                };

                return ApiResponse<object>.SuccessResponse(statusSummary, "Order status summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve order status summary {OrderId}", orderId);
                return ApiResponse<object>.ErrorResponse($"Failed to retrieve order status summary: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateDto)
        {
            try
            {
                _logger.LogInformation("Updating order {OrderId} status to {NewStatus}", orderId, updateDto.Status);
                _logger.LogInformation("UpdateDto received: Status={Status}, Notes={Notes}", updateDto.Status, updateDto.Notes);
                _logger.LogInformation("Status type: {StatusType}", updateDto.Status.GetType().Name);

                var order = await _orderRepository.GetOrderWithAllRelationsAsync(orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", orderId);
                    return ApiResponse<bool>.ErrorResponse("Order not found", 404);
                }

                _logger.LogInformation("Current order status: {CurrentStatus}, New status: {NewStatus}", order.Status, updateDto.Status);
                _logger.LogInformation("Current status type: {CurrentStatusType}, New status type: {NewStatusType}", 
                    order.Status.GetType().Name, updateDto.Status.GetType().Name);

                // Validate status transition
                if (!IsValidStatusTransition(order.Status, updateDto.Status))
                {
                    _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus}", order.Status, updateDto.Status);
                    _logger.LogWarning("Valid transitions from {CurrentStatus}:", order.Status);
                    if (order.Status == OrderStatus.Paid)
                    {
                        _logger.LogWarning("From Paid: Shipped, Cancelled");
                    }
                    else if (order.Status == OrderStatus.Shipped)
                    {
                        _logger.LogWarning("From Shipped: Completed, Cancelled");
                    }
                    else
                    {
                        _logger.LogWarning("No valid transitions from current status");
                    }
                    return ApiResponse<bool>.ErrorResponse("Invalid status transition", 400);
                }

                order.Status = updateDto.Status;

                // Update ShippingStatus based on OrderStatus change
                if (order.ShippingInfo != null)
                {
                    switch (updateDto.Status)
                    {
                        case OrderStatus.Shipped:
                            // Only update to InTransit if current status is Pending
                            if (order.ShippingInfo.Status == ShippingStatus.Pending)
                            {
                                order.ShippingInfo.Status = ShippingStatus.InTransit;
                                _logger.LogInformation("Order {OrderId} marked as shipped and in transit", orderId);
                            }
                            else
                            {
                                _logger.LogInformation("Order {OrderId} status updated to Shipped (shipping status unchanged)", orderId);
                            }
                            break;
                        case OrderStatus.Completed:
                            order.CompletedAt = DateTime.UtcNow;
                            order.ShippingInfo.Status = ShippingStatus.Delivered;
                            _logger.LogInformation("Order {OrderId} marked as completed and delivered", orderId);
                            break;
                        case OrderStatus.Cancelled:
                            order.ShippingInfo.Status = ShippingStatus.Failed;
                            _logger.LogInformation("Order {OrderId} marked as cancelled and shipping failed", orderId);
                            break;
                        default:
                            _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, updateDto.Status);
                            break;
                    }
                }

                // Additional validation: Ensure shipping status is consistent with order status
                if (order.ShippingInfo != null)
                {
                    var expectedShippingStatus = updateDto.Status switch
                    {
                        OrderStatus.Paid => ShippingStatus.Pending,
                        OrderStatus.Shipped => ShippingStatus.InTransit,
                        OrderStatus.Completed => ShippingStatus.Delivered,
                        OrderStatus.Cancelled => ShippingStatus.Failed,
                        _ => order.ShippingInfo.Status
                    };

                    if (order.ShippingInfo.Status != expectedShippingStatus)
                    {
                        _logger.LogInformation("Auto-correcting shipping status from {Current} to {Expected} for order {OrderId}", 
                            order.ShippingInfo.Status, expectedShippingStatus, orderId);
                        order.ShippingInfo.Status = expectedShippingStatus;
                    }
                }

                // Save changes to database
                await _orderRepository.UpdateAsync(order);
                
                _logger.LogInformation("Order {OrderId} status updated successfully to {NewStatus}", orderId, updateDto.Status);
                return ApiResponse<bool>.SuccessResponse(true, "Order status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update order {OrderId} status", orderId);
                return ApiResponse<bool>.ErrorResponse($"Failed to update order status: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<bool>> UpdateShippingStatusAsync(int orderId, UpdateDeliveryShippingStatusDto updateDto)
        {
            try
            {
                _logger.LogInformation("Updating shipping status for order {OrderId} to {NewStatus}", orderId, updateDto.Status);

                var order = await _orderRepository.GetOrderWithAllRelationsAsync(orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", orderId);
                    return ApiResponse<bool>.ErrorResponse("Order not found", 404);
                }

                if (order.ShippingInfo == null)
                {
                    _logger.LogWarning("Order {OrderId} has no shipping info", orderId);
                    return ApiResponse<bool>.ErrorResponse("Order has no shipping information", 400);
                }

                // Validate shipping status transition
                if (!IsValidShippingStatusTransition(order.ShippingInfo.Status, updateDto.Status))
                {
                    _logger.LogWarning("Invalid shipping status transition from {CurrentStatus} to {NewStatus}", 
                        order.ShippingInfo.Status, updateDto.Status);
                    return ApiResponse<bool>.ErrorResponse("Invalid shipping status transition", 400);
                }

                // Update shipping status
                order.ShippingInfo.Status = updateDto.Status;

                // Update order status based on shipping status if needed
                switch (updateDto.Status)
                {
                    case ShippingStatus.InTransit:
                        if (order.Status == OrderStatus.Paid)
                        {
                            order.Status = OrderStatus.Shipped;
                            _logger.LogInformation("Order {OrderId} automatically updated to Shipped", orderId);
                        }
                        break;
                    case ShippingStatus.Delivered:
                        order.Status = OrderStatus.Completed;
                        order.CompletedAt = DateTime.UtcNow;
                        _logger.LogInformation("Order {OrderId} automatically updated to Completed", orderId);
                        break;
                    case ShippingStatus.Failed:
                        order.Status = OrderStatus.Cancelled;
                        _logger.LogInformation("Order {OrderId} automatically updated to Cancelled", orderId);
                        break;
                    case ShippingStatus.Pending:
                        // Don't change order status if shipping is back to pending
                        _logger.LogInformation("Order {OrderId} shipping status set to Pending", orderId);
                        break;
                }

                // Additional validation: If order is completed but shipping is not delivered
                if (order.Status == OrderStatus.Completed && order.ShippingInfo?.Status != ShippingStatus.Delivered)
                {
                    _logger.LogWarning("Order {OrderId} is completed but shipping status is {ShippingStatus}. Auto-correcting...", 
                        orderId, order.ShippingInfo?.Status);
                    order.ShippingInfo.Status = ShippingStatus.Delivered;
                }

                // Additional validation: If shipping is delivered but order is not completed
                if (order.ShippingInfo?.Status == ShippingStatus.Delivered && order.Status != OrderStatus.Completed)
                {
                    _logger.LogWarning("Shipping {OrderId} is delivered but order status is {OrderStatus}. Auto-correcting...", 
                        orderId, order.Status);
                    order.Status = OrderStatus.Completed;
                    order.CompletedAt = DateTime.UtcNow;
                }

                // Save changes to database
                await _orderRepository.UpdateAsync(order);
                
                _logger.LogInformation("Shipping status for order {OrderId} updated successfully to {NewStatus}", orderId, updateDto.Status);
                return ApiResponse<bool>.SuccessResponse(true, "Shipping status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update shipping status for order {OrderId}", orderId);
                return ApiResponse<bool>.ErrorResponse($"Failed to update shipping status: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<bool>> CreateDeliveryAccountAsync()
        {
            try
            {
                // Check if Delivery role exists
                var deliveryRole = await _roleManager.FindByNameAsync("Delivery");
                if (deliveryRole == null)
                {
                    deliveryRole = new ApplicationRole
                    {
                        Name = "Delivery",
                        Description = "Delivery personnel role"
                    };
                    await _roleManager.CreateAsync(deliveryRole);
                }

                // Check if delivery user already exists
                var existingUser = await _userManager.FindByEmailAsync("delivery@bikya.com");
                if (existingUser != null)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "Delivery account already exists");
                }

                // Create delivery user
                var deliveryUser = new ApplicationUser
                {
                    UserName = "delivery@bikya.com",
                    Email = "delivery@bikya.com",
                    FullName = "Delivery Personnel",
                    PhoneNumber = "1234567890",
                    IsVerified = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(deliveryUser, "Delivery@123");
                if (!result.Succeeded)
                {
                    return ApiResponse<bool>.ErrorResponse($"Failed to create delivery account: {string.Join(", ", result.Errors.Select(e => e.Description))}", 400);
                }

                // Assign Delivery role
                await _userManager.AddToRoleAsync(deliveryUser, "Delivery");

                return ApiResponse<bool>.SuccessResponse(true, "Delivery account created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create delivery account");
                return ApiResponse<bool>.ErrorResponse($"Failed to create delivery account: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<bool>> SynchronizeOrderStatusAsync(int orderId)
        {
            try
            {
                _logger.LogInformation("Synchronizing order status for order {OrderId}", orderId);

                var order = await _orderRepository.GetOrderWithAllRelationsAsync(orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", orderId);
                    return ApiResponse<bool>.ErrorResponse("Order not found", 404);
                }

                if (order.ShippingInfo == null)
                {
                    _logger.LogWarning("Order {OrderId} has no shipping info", orderId);
                    return ApiResponse<bool>.ErrorResponse("Order has no shipping information", 400);
                }

                var originalOrderStatus = order.Status;
                var originalShippingStatus = order.ShippingInfo.Status;
                var changesMade = false;

                // Synchronize based on order status
                var expectedShippingStatus = order.Status switch
                {
                    OrderStatus.Paid => ShippingStatus.Pending,
                    OrderStatus.Shipped => ShippingStatus.InTransit,
                    OrderStatus.Completed => ShippingStatus.Delivered,
                    OrderStatus.Cancelled => ShippingStatus.Failed,
                    _ => order.ShippingInfo.Status
                };

                if (order.ShippingInfo.Status != expectedShippingStatus)
                {
                    _logger.LogInformation("Correcting shipping status from {Current} to {Expected} for order {OrderId}", 
                        order.ShippingInfo.Status, expectedShippingStatus, orderId);
                    order.ShippingInfo.Status = expectedShippingStatus;
                    changesMade = true;
                }

                // Synchronize based on shipping status
                var expectedOrderStatus = order.ShippingInfo.Status switch
                {
                    ShippingStatus.Pending => OrderStatus.Paid,
                    ShippingStatus.InTransit => OrderStatus.Shipped,
                    ShippingStatus.Delivered => OrderStatus.Completed,
                    ShippingStatus.Failed => OrderStatus.Cancelled,
                    _ => order.Status
                };

                if (order.Status != expectedOrderStatus)
                {
                    _logger.LogInformation("Correcting order status from {Current} to {Expected} for order {OrderId}", 
                        order.Status, expectedOrderStatus, orderId);
                    order.Status = expectedOrderStatus;
                    
                    if (expectedOrderStatus == OrderStatus.Completed)
                    {
                        order.CompletedAt = DateTime.UtcNow;
                    }
                    
                    changesMade = true;
                }

                if (changesMade)
                {
                    await _orderRepository.UpdateAsync(order);
                    _logger.LogInformation("Order {OrderId} status synchronized successfully", orderId);
                    return ApiResponse<bool>.SuccessResponse(true, "Order status synchronized successfully");
                }
                else
                {
                    _logger.LogInformation("Order {OrderId} status is already synchronized", orderId);
                    return ApiResponse<bool>.SuccessResponse(true, "Order status is already synchronized");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to synchronize order status for order {OrderId}", orderId);
                return ApiResponse<bool>.ErrorResponse($"Failed to synchronize order status: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<object>> GetAvailableTransitionsAsync(int orderId)
        {
            try
            {
                _logger.LogInformation("Getting available transitions for order {OrderId}", orderId);

                var order = await _orderRepository.GetOrderWithAllRelationsAsync(orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", orderId);
                    return ApiResponse<object>.ErrorResponse("Order not found", 404);
                }

                var availableTransitions = new
                {
                    OrderId = order.Id,
                    CurrentOrderStatus = order.Status,
                    CurrentShippingStatus = order.ShippingInfo?.Status ?? ShippingStatus.Pending,
                    OrderStatusTransitions = GetOrderStatusTransitions(order.Status),
                    ShippingStatusTransitions = order.ShippingInfo != null ? GetShippingStatusTransitions(order.ShippingInfo.Status) : new string[0],
                    Recommendations = GetTransitionRecommendations(order.Status, order.ShippingInfo?.Status)
                };

                return ApiResponse<object>.SuccessResponse(availableTransitions, "Available transitions retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available transitions for order {OrderId}", orderId);
                return ApiResponse<object>.ErrorResponse($"Failed to get available transitions: {ex.Message}", 500);
            }
        }

        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            _logger.LogInformation("Validating status transition: {CurrentStatus} -> {NewStatus}", currentStatus, newStatus);
            
            // Define valid transitions
            var validTransitions = new Dictionary<OrderStatus, OrderStatus[]>
            {
                { OrderStatus.Pending, new[] { OrderStatus.Paid, OrderStatus.Cancelled } },
                { OrderStatus.Paid, new[] { OrderStatus.Shipped, OrderStatus.Cancelled } },
                { OrderStatus.Shipped, new[] { OrderStatus.Completed, OrderStatus.Cancelled } },
                { OrderStatus.Completed, new OrderStatus[] { } }, // No further transitions allowed
                { OrderStatus.Cancelled, new OrderStatus[] { } }   // No further transitions allowed
            };

            if (!validTransitions.ContainsKey(currentStatus))
            {
                _logger.LogWarning("Unknown current status: {CurrentStatus}", currentStatus);
                return false;
            }

            var allowedTransitions = validTransitions[currentStatus];
            var isValid = allowedTransitions.Contains(newStatus);
            
            _logger.LogInformation("Status transition validation result: {IsValid}", isValid);
            if (!isValid)
            {
                _logger.LogWarning("Invalid transition from {CurrentStatus} to {NewStatus}. Allowed transitions: {AllowedTransitions}", 
                    currentStatus, newStatus, string.Join(", ", allowedTransitions));
            }
            
            return isValid;
        }

        private bool IsValidShippingStatusTransition(ShippingStatus currentStatus, ShippingStatus newStatus)
        {
            _logger.LogInformation("Validating shipping status transition: {CurrentStatus} -> {NewStatus}", currentStatus, newStatus);
            
            // Define valid shipping status transitions
            var validTransitions = new Dictionary<ShippingStatus, ShippingStatus[]>
            {
                { ShippingStatus.Pending, new[] { ShippingStatus.InTransit, ShippingStatus.Failed, ShippingStatus.Delivered } }, // Allow direct to Delivered
                { ShippingStatus.InTransit, new[] { ShippingStatus.Delivered, ShippingStatus.Failed, ShippingStatus.Pending } }, // Allow going back to pending
                { ShippingStatus.Delivered, new ShippingStatus[] { } }, // No further transitions allowed
                { ShippingStatus.Failed, new[] { ShippingStatus.Pending, ShippingStatus.Delivered } } // Allow retry from failed
            };

            if (!validTransitions.ContainsKey(currentStatus))
            {
                _logger.LogWarning("Unknown current shipping status: {CurrentStatus}", currentStatus);
                return false;
            }

            var allowedTransitions = validTransitions[currentStatus];
            var isValid = allowedTransitions.Contains(newStatus);
            
            _logger.LogInformation("Shipping status transition validation result: {IsValid}", isValid);
            if (!isValid)
            {
                _logger.LogWarning("Invalid shipping status transition from {CurrentStatus} to {NewStatus}. Allowed transitions: {AllowedTransitions}", 
                    currentStatus, newStatus, string.Join(", ", allowedTransitions));
            }
            
            return isValid;
        }

        private bool IsStatusSynchronized(OrderStatus orderStatus, ShippingStatus? shippingStatus)
        {
            if (shippingStatus == null) return false;
            
            return (orderStatus, shippingStatus) switch
            {
                (OrderStatus.Paid, ShippingStatus.Pending) => true,
                (OrderStatus.Shipped, ShippingStatus.InTransit) => true,
                (OrderStatus.Completed, ShippingStatus.Delivered) => true,
                (OrderStatus.Cancelled, ShippingStatus.Failed) => true,
                _ => false
            };
        }

        private object GetNextAllowedTransitions(OrderStatus orderStatus, ShippingStatus? shippingStatus)
        {
            var orderTransitions = GetOrderStatusTransitions(orderStatus);
            var shippingTransitions = shippingStatus.HasValue ? GetShippingStatusTransitions(shippingStatus.Value) : new string[0];

            return new
            {
                OrderStatus = orderTransitions,
                ShippingStatus = shippingTransitions
            };
        }

        private string[] GetOrderStatusTransitions(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => new[] { "Paid", "Cancelled" },
                OrderStatus.Paid => new[] { "Shipped", "Cancelled" },
                OrderStatus.Shipped => new[] { "Completed", "Cancelled" },
                _ => new string[0]
            };
        }

        private string[] GetShippingStatusTransitions(ShippingStatus status)
        {
            return status switch
            {
                ShippingStatus.Pending => new[] { "InTransit", "Failed" },
                ShippingStatus.InTransit => new[] { "Delivered", "Failed", "Pending" },
                ShippingStatus.Failed => new[] { "Pending" },
                _ => new string[0]
            };
        }

        private object GetTransitionRecommendations(OrderStatus orderStatus, ShippingStatus? shippingStatus)
        {
            var recommendations = new List<string>();

            if (orderStatus == OrderStatus.Completed && shippingStatus == ShippingStatus.Pending)
            {
                recommendations.Add("Order is completed but shipping is pending. Consider updating shipping status to 'Delivered'");
            }
            else if (orderStatus == OrderStatus.Paid && shippingStatus == ShippingStatus.Pending)
            {
                recommendations.Add("Order is paid. Consider updating shipping status to 'InTransit' or 'Delivered'");
            }
            else if (shippingStatus == ShippingStatus.Delivered && orderStatus != OrderStatus.Completed)
            {
                recommendations.Add("Shipping is delivered. Order status should be 'Completed'");
            }
            else if (shippingStatus == ShippingStatus.Failed && orderStatus != OrderStatus.Cancelled)
            {
                recommendations.Add("Shipping failed. Consider cancelling the order");
            }

            return recommendations;
        }
    }
}

