using DepiFinalProject.DTOs;
using DepiFinalProject.Interfaces;
using DepiFinalProject.Models;
using DepiFinalProject.Repositories;
using static DepiFinalProject.DTOs.OrderDto;

namespace DepiFinalProject.Services
{
    public class OrderService : IOrderService
    {
        protected readonly IOrderRepository _orderRepository;
        protected readonly IProductRepository _productRepository;
        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }
        public async Task<OrderResponseDTO?> CreateAsync(CreateOrderDTO dto)
        {
            if (dto.OrderItems == null || !dto.OrderItems.Any())
                throw new ArgumentException("Order items cannot be empty");
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in dto.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {item.ProductId} not found.");
                if (product.Stock < item.Quantity)
                    throw new InvalidOperationException($"Not enough stock for product {product.ProductName}.");

                var orderItem = new OrderItem
                {
                    ProductID = item.ProductId,
                    ProductName = product.ProductName,
                    Quantity = item.Quantity,
                    Price = product.Price
                };

                totalAmount += orderItem.Quantity * orderItem.Price;
                orderItems.Add(orderItem);

                product.Stock -= item.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            var order = new Order
            {
                UserID = dto.UserId,
                OrderNo = GenerateOrderNumber(),
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                OrderStatus = "Pending",
                OrderItems = orderItems
            };

            var createdOrder = await _orderRepository.CreateAsync(order);
            return MapToResponseDto(createdOrder);
        }

        public async Task<bool> CancelAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.OrderStatus == "Cancelled " || order.OrderStatus == "Delivered")
                return false;

            foreach (var item in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductID);
                if (product != null)
                {
                    product.Stock += item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
            }

            order.OrderStatus = "Cancelled";
            await _orderRepository.UpdateAsync(order);
            return true;
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetAllAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToResponseDto);
        }

        public async Task<OrderDetailsDTO?> GetById(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");
            return MapToDetailDto(order);
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetByUserAsync(int userId)
        {
            var orders = await _orderRepository.GetByUserAsync(userId);
            return orders.Select(MapToResponseDto);
        }

        public async Task<OrderResponseDTO?> UpdateStatusAsync(int orderId, UpdateOrderStatusDTO dto)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            order.OrderStatus = dto.OrderStatus;
            var updatedOrder = await _orderRepository.UpdateAsync(order);
            return updatedOrder == null ? null : MapToResponseDto(updatedOrder);
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
        public async Task<IEnumerable<OrderItemResponseDTO>> GetOrderItemsAsync(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID");

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            return order.OrderItems?.Select(MapToOrderItemResponseDto)
                   ?? new List<OrderItemResponseDTO>();
        }
        public async Task<OrderItemResponseDTO?> AddOrderItemAsync(int orderId, AddOrderItemDTO dto)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID");

            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Get the order
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            // Check if order can be modified
            if (order.OrderStatus != "Pending")
                throw new InvalidOperationException($"Cannot add items to order with status: {order.OrderStatus}");

            // Get the product
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {dto.ProductId} not found.");

            // Check stock
            if (product.Stock < dto.Quantity)
                throw new InvalidOperationException($"Not enough stock for product {product.ProductName}. Available: {product.Stock}, Requested: {dto.Quantity}");

            // Create order item
            var orderItem = new OrderItem
            {
                OrderID = orderId,
                ProductID = dto.ProductId,
                ProductName = product.ProductName,
                Quantity = dto.Quantity,
                Price = product.Price
            };

            // Add order item using repository
            var createdOrderItem = await _orderRepository.AddOrderItemAsync(orderItem);

            // Update order total
            order.TotalAmount += orderItem.Quantity * orderItem.Price;

            // Update product stock
            product.Stock -= dto.Quantity;

            // Save changes to order and product
            await _orderRepository.UpdateAsync(order);
            await _productRepository.UpdateAsync(product);

            return MapToOrderItemResponseDto(createdOrderItem);
        }

       


        private OrderResponseDTO MapToResponseDto(Order order)
        {
            return new OrderResponseDTO
            {
                OrderID = order.OrderID,
                UserID = order.UserID,
                UserName = order.User == null ? "Unknown" : $"{order.User.UserFirstName} {order.User.UserLastName}",
                OrderNo = order.OrderNo,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus
            };
        }



        private OrderDetailsDTO MapToDetailDto(Order order)
        {
            return new OrderDetailsDTO
            {
                OrderID = order.OrderID,
                UserID = order.UserID,
                UserName = order.User == null ? "Unknown" : $"{order.User.UserFirstName} {order.User.UserLastName}",
                OrderNo = order.OrderNo,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus,
                OrderItems = order.OrderItems?.Select(oi => new OrderItemDetailsDTO
                {
                    OrderItemID = oi.OrderItemID,
                    ProductID = oi.ProductID,
                    ProductName = oi.Product?.ProductName ?? "Unknown",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.Price,
                    TotalPrice = oi.Quantity * oi.Price
                }).ToList() ?? new List<OrderItemDetailsDTO>()
            };
        }
        private OrderItemResponseDTO MapToOrderItemResponseDto(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            return new OrderItemResponseDTO
            {
                OrderItemID = orderItem.OrderItemID,
                OrderID = orderItem.OrderID,
                ProductID = orderItem.ProductID,
                ProductName = orderItem.ProductName,
                Quantity = orderItem.Quantity,
                Price = orderItem.Price,
                TotalPrice = orderItem.Quantity * orderItem.Price
            };
        }
        public Task<Order> GetByIdAsync(int orderId)
        {
            return _orderRepository.GetByIdAsync(orderId);
        }
    }
}
