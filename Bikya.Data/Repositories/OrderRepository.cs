﻿
using Bikya.Data;
using Bikya.Data.Enums;
using Bikya.Data.Models;
using Bikya.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bikya.Data.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private new readonly BikyaContext _context;

        public OrderRepository(BikyaContext context, ILogger<OrderRepository> logger) : base(context, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Order?> GetOrderWithAllRelationsAsync(int orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.Product)
                    .Include(o => o.Buyer)
                    .Include(o => o.Seller)
                    .Include(o => o.ShippingInfo)
                    .Include(o => o.Reviews)
                    .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order with ID {OrderId} and all relations", orderId);
                throw;
            }
        }

        public async Task<Order?> GetOrderWithShippingInfoAsync(int orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.ShippingInfo)
                    .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order with ID {OrderId} and shipping info", orderId);
                throw;
            }
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.Product)
                    .Include(o => o.Buyer)
                    .Include(o => o.Seller)
                    .Where(o => o.BuyerId == userId || o.SellerId == userId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<Order>> GetOrdersByBuyerIdAsync(int buyerId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.Product)
                    .Include(o => o.Buyer)
                    .Include(o => o.Seller)
                    .Where(o => o.BuyerId == buyerId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for buyer {BuyerId}", buyerId);
                throw;
            }
        }

        public async Task<List<Order>> GetOrdersBySellerIdAsync(int sellerId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.Product)
                    .Include(o => o.Buyer)
                    .Include(o => o.Seller)
                    .Where(o => o.SellerId == sellerId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for seller {SellerId}", sellerId);
                throw;
            }
        }

        public async Task<List<Order>> GetAllOrdersWithRelationsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.Product)
                    .Include(o => o.Buyer)
                    .Include(o => o.Seller)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders with relations");
                throw;
            }
        }

        public async Task<bool> CanUserCancelOrderAsync(int orderId, int buyerId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .AnyAsync(o => o.Id == orderId && o.BuyerId == buyerId && o.Status == OrderStatus.Pending, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {BuyerId} can cancel order {OrderId}", buyerId, orderId);
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _context.Orders.FindAsync(new object[] { orderId }, cancellationToken);
                if (order == null)
                    return false;

                order.Status = newStatus;

                if (newStatus == OrderStatus.Paid)
                    order.PaidAt = DateTime.UtcNow;
                else if (newStatus == OrderStatus.Completed)
                    order.CompletedAt = DateTime.UtcNow;

                _context.Orders.Update(order);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} status to {NewStatus}", orderId, newStatus);
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.Product)
                    .Include(o => o.Buyer)
                    .Include(o => o.Seller)
                    .Where(o => o.Status == status)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by status {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.Product)
                    .Include(o => o.Buyer)
                    .Include(o => o.Seller)
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by date range {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.Status == OrderStatus.Completed)
                    .SumAsync(o => o.PlatformFee, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total revenue");
                throw;
            }
        }

        public async Task<decimal> GetSellerRevenueAsync(int sellerId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.SellerId == sellerId && o.Status == OrderStatus.Completed)
                    .SumAsync(o => o.SellerAmount, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating revenue for seller {SellerId}", sellerId);
                throw;
            }
        }

        public async Task<int> GetOrdersCountByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Orders
                    .AsNoTracking()
                    .CountAsync(o => o.Status == status, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting orders by status {Status}", status);
                throw;
            }
        }

        public override async Task AddAsync(Order entity, CancellationToken cancellationToken = default)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.Status = OrderStatus.Pending;
                await base.AddAsync(entity, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding order");
                throw;
            }
        }

        public override void Update(Order entity)
        {
            try
            {
                base.Update(entity);
                
                // Preserve CreatedAt field during updates
                _context.Entry(entity).Property(e => e.CreatedAt).IsModified = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", entity.Id);
                throw;
            }
        }
    }
}