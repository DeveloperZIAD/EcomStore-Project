using Data_layer.Conation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data_layer
{
    // POCO class for Order Items
    public class clsorderitem
    {
        public int id { get; set; }
        public int order_id { get; set; }
        public int product_id { get; set; }
        public int quantity { get; set; }
        public decimal price_at_purchase { get; set; }  // Snapshot of price at time of order
    }

    // Data Access Layer for Order Items
    public static class orderitem_dal
    {
        // CREATE - Add new order item (returns the new item ID)
        public static int AddOrderItem(clsorderitem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            string sql = @"
                INSERT INTO order_items (order_id, product_id, quantity, price_at_purchase)
                VALUES (@order_id, @product_id, @quantity, @price_at_purchase);
                SELECT CAST(SCOPE_IDENTITY() AS int);";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@order_id", item.order_id);
            cmd.Parameters.AddWithValue("@product_id", item.product_id);
            cmd.Parameters.AddWithValue("@quantity", item.quantity);
            cmd.Parameters.AddWithValue("@price_at_purchase", item.price_at_purchase);

            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        // CREATE - Add multiple order items at once (useful for checkout)
        public static void AddOrderItems(List<clsorderitem> items)
        {
            if (items == null || items.Count == 0) return;

            string sql = @"
                INSERT INTO order_items (order_id, product_id, quantity, price_at_purchase)
                VALUES (@order_id, @product_id, @quantity, @price_at_purchase);";

            using var conn = ConnectionManager.GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                foreach (var item in items)
                {
                    using var cmd = new SqlCommand(sql, conn, transaction);
                    cmd.Parameters.AddWithValue("@order_id", item.order_id);
                    cmd.Parameters.AddWithValue("@product_id", item.product_id);
                    cmd.Parameters.AddWithValue("@quantity", item.quantity);
                    cmd.Parameters.AddWithValue("@price_at_purchase", item.price_at_purchase);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // READ - Get all items for a specific order
        public static List<clsorderitem> GetOrderItemsByOrderId(int orderId)
        {
            var list = new List<clsorderitem>();

            string sql = @"
                SELECT id, order_id, product_id, quantity, price_at_purchase
                FROM order_items
                WHERE order_id = @order_id
                ORDER BY id ASC;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@order_id", orderId);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new clsorderitem
                {
                    id = reader.GetInt32("id"),
                    order_id = reader.GetInt32("order_id"),
                    product_id = reader.GetInt32("product_id"),
                    quantity = reader.GetInt32("quantity"),
                    price_at_purchase = reader.GetDecimal("price_at_purchase")
                });
            }

            return list;
        }

        // READ - Get single order item by ID
        public static clsorderitem GetOrderItemById(int itemId)
        {
            string sql = @"
                SELECT id, order_id, product_id, quantity, price_at_purchase
                FROM order_items
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", itemId);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new clsorderitem
                {
                    id = reader.GetInt32("id"),
                    order_id = reader.GetInt32("order_id"),
                    product_id = reader.GetInt32("product_id"),
                    quantity = reader.GetInt32("quantity"),
                    price_at_purchase = reader.GetDecimal("price_at_purchase")
                };
            }

            return null; // Not found
        }

        // UPDATE - Update quantity or price (rare, but possible for admin corrections)
        public static bool UpdateOrderItem(clsorderitem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            string sql = @"
                UPDATE order_items
                SET quantity = @quantity,
                    price_at_purchase = @price_at_purchase
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", item.id);
            cmd.Parameters.AddWithValue("@quantity", item.quantity);
            cmd.Parameters.AddWithValue("@price_at_purchase", item.price_at_purchase);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // DELETE - Delete a single order item
        public static bool DeleteOrderItem(int itemId)
        {
            string sql = "DELETE FROM order_items WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", itemId);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // DELETE - Delete all items for an order (useful when canceling order)
        public static bool DeleteOrderItemsByOrderId(int orderId)
        {
            string sql = "DELETE FROM order_items WHERE order_id = @order_id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@order_id", orderId);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // Helper: Calculate total amount for an order from its items
        public static decimal GetOrderTotalFromItems(int orderId)
        {
            string sql = @"
                SELECT SUM(quantity * price_at_purchase)
                FROM order_items
                WHERE order_id = @order_id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@order_id", orderId);

            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
        }
    }
}