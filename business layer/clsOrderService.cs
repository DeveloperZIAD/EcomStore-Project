// File: OrderService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Data_layer;
using Business_layer.Dtos;

namespace Business_layer
{
    public static class OrderService
    {
        /// <summary>
        /// Creates a new order (typically called after payment confirmation)
        /// </summary>
        public static int CreateOrder(int userId, int? addressId, decimal totalAmount, string status = "pending")
        {
            ValidateUserId(userId);
            if (totalAmount < 0) throw new ArgumentException("Total amount cannot be negative.");
            if (addressId.HasValue && addressId <= 0) throw new ArgumentException("Invalid address ID.");

            var order = new clsorder
            {
                user_id = userId,
                address_id = addressId,
                total_amount = totalAmount,
                status = status
            };

            int newOrderId = orderDal.AddOrder(order);

            if (newOrderId <= 0)
                throw new Exception("Failed to create the order.");

            AuditLogService.LogAction("Order Created", $"Order ID: {newOrderId}, User ID: {userId}, Amount: {totalAmount}");

            return newOrderId;
        }

        public static FullOrderDetailsDto GetOrderDetails(int orderId)
        {
            var order = orderDal.GetOrderById(orderId);
            if (order == null) return null;

            var items = orderitem_dal.GetOrderItemsByOrderId(orderId);
            var payment = orderDal.GetPaymentByOrderId(orderId); // دلوقتي شغالة
            var address = order.address_id.HasValue ? addressesdb_dal.GetAddressById(order.address_id.Value) : null;
            var user = user_dal.GetUserById(order.user_id);

            return new FullOrderDetailsDto
            {
                Order = order,
                Items = items,
                Payment = payment,
                Address = address,
                User = user
            };
        }

        /// <summary>
        /// Retrieves all orders for a specific user (customer view)
        /// </summary>
        public static List<OrderResponseDto> GetOrdersByUserId(int userId)
        {
            ValidateUserId(userId);

            var dbOrders = orderDal.GetOrdersByUserId(userId);

            return dbOrders.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Retrieves a single order by ID (with ownership check for customers)
        /// </summary>
        public static OrderResponseDto GetOrderById(int orderId, int userId)
        {
            if (orderId <= 0) throw new ArgumentException("Invalid order ID.");
            ValidateUserId(userId);

            var order = orderDal.GetOrderById(orderId);

            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            if (order.user_id != userId)
                throw new UnauthorizedAccessException("You are not authorized to view this order.");

            return MapToResponseDto(order);
        }

        /// <summary>
        /// Retrieves all orders with user and address details (admin view)
        /// </summary>
        public static List<OrderSummaryResponseDto> GetAllOrdersAdmin()
        {
            var dbOrders = orderDal.GetAllOrders();

            return dbOrders.Select(MapToSummaryDto).ToList();
        }

        /// <summary>
        /// Updates the status of an order (admin only in most cases)
        /// </summary>
        public static bool UpdateOrderStatus(int orderId, string newStatus)
        {
            if (orderId <= 0) throw new ArgumentException("Invalid order ID.");

            var validStatuses = new[] { "pending", "processing", "shipped", "delivered", "cancelled" };
            if (!validStatuses.Contains(newStatus.ToLower()))
                throw new ArgumentException("Invalid order status.");

            bool success = orderDal.UpdateOrderStatus(orderId, newStatus);

            if (success)
            {
                AuditLogService.LogAction("Order Status Updated", $"Order ID: {orderId}, New Status: {newStatus}");
            }

            return success;
        }

        /// <summary>
        /// Cancels an order (allowed only if status is pending or processing)
        /// </summary>
        public static bool CancelOrder(int orderId, int userId)
        {
            var order = GetOrderById(orderId, userId); // Ensures ownership

            if (order.Status != "pending" && order.Status != "processing")
                throw new InvalidOperationException("Order can only be cancelled if it is pending or processing.");

            bool success = UpdateOrderStatus(orderId, "cancelled");

            if (success)
            {
                // Optionally: restore stock here if needed
                AuditLogService.LogAction("Order Cancelled", $"Order ID: {orderId} by User ID: {userId}");
            }

            return success;
        }

        // ----------------- Private Helper Methods -----------------

        private static void ValidateUserId(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");
        }

        private static OrderResponseDto MapToResponseDto(clsorder db)
        {
            return new OrderResponseDto
            {
                Id = db.id,
                UserId = db.user_id,
                AddressId = db.address_id,
                TotalAmount = db.total_amount,
                Status = db.status,
                CreatedAt = db.created_at
            };
        }

        private static OrderSummaryResponseDto MapToSummaryDto(dynamic dbOrder) // dynamic because GetAllOrders returns anonymous type from JOIN
        {
            return new OrderSummaryResponseDto
            {
                Id = dbOrder.order_id,
                Username = dbOrder.username,
                Email = dbOrder.email,
                TotalAmount = dbOrder.total_amount,
                Status = dbOrder.status,
                CreatedAt = dbOrder.created_at,
                FullAddress = string.Join(", ", new[]
                {
                    dbOrder.street,
                    dbOrder.city,
                    dbOrder.state,
                    dbOrder.country,
                    dbOrder.zip_code
                }.Where(s => !string.IsNullOrWhiteSpace(s)))
            };
        }
    }
}