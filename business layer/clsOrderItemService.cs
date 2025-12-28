// File: OrderItemService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Data_layer;

namespace Business_layer
{
    public static class OrderItemService
    {
        public static List<clsorderitem> GetOrderItemsByOrderId(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID.");

            return orderitem_dal.GetOrderItemsByOrderId(orderId);
        }

        public static clsorderitem GetOrderItemById(int itemId)
        {
            if (itemId <= 0)
                throw new ArgumentException("Invalid order item ID.");

            var item = orderitem_dal.GetOrderItemById(itemId);

            if (item == null)
                throw new KeyNotFoundException("Order item not found.");

            return item;
        }

        public static void AddOrderItems(int orderId, List<clsorderitem> items)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID.");

            if (items == null || items.Count == 0)
                throw new ArgumentException("Order items cannot be empty.");

            ValidateOrderItems(items);

            // تعيين order_id لكل عنصر
            foreach (var item in items)
            {
                item.order_id = orderId;
            }

            orderitem_dal.AddOrderItems(items);

            AuditLogService.LogAction("Order Items Added", $"Order ID: {orderId}, Count: {items.Count}");
        }

        public static bool UpdateOrderItem(int itemId, clsorderitem itemDto)
        {
            if (itemId <= 0)
                throw new ArgumentException("Invalid order item ID.");

            ValidateSingleOrderItem(itemDto);

            var existing = orderitem_dal.GetOrderItemById(itemId);

            if (existing == null)
                throw new KeyNotFoundException("Order item not found.");

            existing.product_id = itemDto.product_id;
            existing.quantity = itemDto.quantity;
            existing.price_at_purchase = itemDto.price_at_purchase;

            bool success = orderitem_dal.UpdateOrderItem(existing);

            if (success)
            {
                AuditLogService.LogAction("Order Item Updated", $"Item ID: {itemId}, New Quantity: {itemDto.quantity}");
            }

            return success;
        }

        public static bool DeleteOrderItem(int itemId)
        {
            if (itemId <= 0)
                throw new ArgumentException("Invalid order item ID.");

            var item = orderitem_dal.GetOrderItemById(itemId);

            if (item == null)
                throw new KeyNotFoundException("Order item not found.");

            bool success = orderitem_dal.DeleteOrderItem(itemId);

            if (success)
            {
                AuditLogService.LogAction("Order Item Deleted", $"Item ID: {itemId}, Order ID: {item.order_id}");
            }

            return success;
        }

        public static bool DeleteOrderItemsByOrderId(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID.");

            bool success = orderitem_dal.DeleteOrderItemsByOrderId(orderId);

            if (success)
            {
                AuditLogService.LogAction("All Order Items Deleted", $"Order ID: {orderId}");
            }

            return success;
        }

        private static void ValidateOrderItems(List<clsorderitem> items)
        {
            foreach (var item in items)
            {
                ValidateSingleOrderItem(item);
            }
        }

        private static void ValidateSingleOrderItem(clsorderitem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.product_id <= 0) throw new ArgumentException("Invalid product ID.");
            if (item.quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
            if (item.price_at_purchase < 0) throw new ArgumentException("Price cannot be negative.");
        }
    }
}