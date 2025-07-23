using Bikya.Data.Enums;
using Bikya.Data.Models;

namespace Bikya.Data.Repositories.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order?> GetOrderWithAllRelationsAsync(int orderId, CancellationToken cancellationToken = default);

        Task<Order?> GetOrderWithShippingInfoAsync(int orderId, CancellationToken cancellationToken = default);

        Task<List<Order>> GetOrdersByUserIdAsync(int userId, CancellationToken cancellationToken = default);

        Task<List<Order>> GetOrdersByBuyerIdAsync(int buyerId, CancellationToken cancellationToken = default);

        Task<List<Order>> GetOrdersBySellerIdAsync(int sellerId, CancellationToken cancellationToken = default);

        Task<List<Order>> GetAllOrdersWithRelationsAsync(CancellationToken cancellationToken = default);

        Task<bool> CanUserCancelOrderAsync(int orderId, int buyerId, CancellationToken cancellationToken = default);

        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, CancellationToken cancellationToken = default);

        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);

        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);

        Task<decimal> GetSellerRevenueAsync(int sellerId, CancellationToken cancellationToken = default);

        Task<int> GetOrdersCountByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    }
}