using Ecommerce.Messaging.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Dtos;
using OrderService.Models;

namespace OrderService.Services
{
    public class OrderManagementService : IOrderManagementService
    {
        private readonly OrderDbContext _orderDbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        public OrderManagementService(OrderDbContext dbContext, IPublishEndpoint publishEndpoint)
        {
            _orderDbContext = dbContext;
            _publishEndpoint = publishEndpoint;
        }
        private static OrderItemResponse MapToOrderItemResponse(OrderItem orderItem)
        {
            return new OrderItemResponse
            {
                ProductId = orderItem.ProductId,
                ProductName = orderItem.ProductName,
                UnitPrice = orderItem.UnitPrice,
                Quantity = orderItem.Quantity,
                Discount = orderItem.Discount
            };
        }
        private static OrderResponse MapToOrderResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                Date = order.Date,
                OrderStatus = order.OrderStatus,
                Total = order.Total,
                Items = (order.OrderItems ?? new List<OrderItem>()).Select(MapToOrderItemResponse).ToList()
            };
        }
        public async Task<OrderResponse> CancelAsync(int id)
        {
            UpdateOrderRequest cancelRequest = new UpdateOrderRequest
            {
                OrderStatus = OrderStatus.Cancelled
            };
            return await UpdateStatusAsync(id, cancelRequest);
        }

        public async Task<OrderResponse> CreateAsync(CreateOrderRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.UserId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(request.UserId), "UserId must be greater than zero.");
            }

            if (request.Items is null || request.Items.Count == 0)
            {
                throw new ArgumentException("Order must contain at least one item.", nameof(request.Items));
            }

            if (request.Items.Any(i => i.Discount < 0 || i.Discount > 1))
            {
                throw new ArgumentOutOfRangeException(nameof(request.Items), "Discount must be a value between 0 and 1 (e.g. 0.1 for 10%).");
            }


            var orderItems = request.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Discount = item.Discount
            }).ToList();

            var total = orderItems.Sum(item => item.UnitPrice * item.Quantity * (1 - item.Discount));

            var order = new Order
            {
                UserId = request.UserId,
                Date = DateTime.UtcNow,
                OrderStatus = OrderStatus.Pending,
                Total = total,
                OrderItems = orderItems,
            };

            _orderDbContext.Add(order);
            await _orderDbContext.SaveChangesAsync();

            await _publishEndpoint.Publish(new OrderPlacedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                Amount = total
            });

            return MapToOrderResponse(order);
        }

        public async Task<List<OrderResponse>> GetAllAsync()
        {

            var orders = await _orderDbContext.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.Date)
                .ToListAsync();

            return orders.Select(MapToOrderResponse).ToList();

        }

        public async Task<OrderResponse> GetByIdAsync(int id)
        {
            var order = await _orderDbContext.Orders.AsNoTracking()
            .Include(o => o.OrderItems).SingleOrDefaultAsync(o => o.Id == id);

            if (order is null)
            {
                throw new KeyNotFoundException($"Order with ID {id} could not be found.");
            }

            return MapToOrderResponse(order);
        }

        public async Task<List<OrderResponse>> GetByUserIdAsync(int userId)
        {
            var orders = await _orderDbContext.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.Date)
                .ToListAsync();

            return orders.Select(MapToOrderResponse).ToList();
        }

        public async Task<OrderResponse> UpdateStatusAsync(int id, UpdateOrderRequest request)
        {
            var order = await _orderDbContext.Orders
                .Include(o => o.OrderItems)
                .SingleOrDefaultAsync(o => o.Id == id);

            if (order is null)
            {
                throw new KeyNotFoundException($"Order with ID {id} not found.");
            }

            if (order.OrderStatus == OrderStatus.Delivered || order.OrderStatus == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException($"Cannot modify order in terminal state: {order.OrderStatus}");
            }

            bool isValidTransition = (order.OrderStatus, request.OrderStatus) switch
            {
                (OrderStatus.Pending, OrderStatus.Paid) => true,
                (OrderStatus.Pending, OrderStatus.Cancelled) => true,

                (OrderStatus.Paid, OrderStatus.Shipped) => true,
                (OrderStatus.Paid, OrderStatus.Cancelled) => true,

                (OrderStatus.Shipped, OrderStatus.Delivered) => true,

                _ => false
            };

            if (!isValidTransition)
            {
                throw new InvalidOperationException($"Illegal transition: {order.OrderStatus} -> {request.OrderStatus}");
            }

            order.OrderStatus = request.OrderStatus;
            await _orderDbContext.SaveChangesAsync();

            return MapToOrderResponse(order);
        }
    }
}