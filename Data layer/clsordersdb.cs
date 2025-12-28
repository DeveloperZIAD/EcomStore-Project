using Data_layer.Conation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data_layer
{
    // POCO class for Orders
    public class clsorder
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int? address_id { get; set; }      // Nullable
        public decimal total_amount { get; set; }
        public string status { get; set; }        // pending, processing, shipped, delivered, cancelled
        public DateTime created_at { get; set; }
    }

    // Data Access Layer for Orders
    public static partial class orderDal
    {
        // CREATE - Add new order (returns the new order ID)
        public static int AddOrder(clsorder order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            string sql = @"
                INSERT INTO orders (user_id, address_id, total_amount, status)
                VALUES (@user_id, @address_id, @total_amount, @status);
                SELECT CAST(SCOPE_IDENTITY() AS int);";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@user_id", order.user_id);
            cmd.Parameters.AddWithValue("@address_id", (object)order.address_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@total_amount", order.total_amount);
            cmd.Parameters.AddWithValue("@status", order.status ?? "pending");

            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        // READ - Get order by ID
        public static clsorder GetOrderById(int orderId)
        {
            string sql = @"
                SELECT id, user_id, address_id, total_amount, status, created_at
                FROM orders
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", orderId);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new clsorder
                {
                    id = reader.GetInt32("id"),
                    user_id = reader.GetInt32("user_id"),
                    address_id = reader.IsDBNull("address_id") ? (int?)null : reader.GetInt32("address_id"),
                    total_amount = reader.GetDecimal("total_amount"),
                    status = reader.GetString("status"),
                    created_at = reader.GetDateTime("created_at")
                };
            }

            return null; // Not found
        }

        // READ - Get all orders for a specific user (including guests)
        public static List<clsorder> GetOrdersByUserId(int userId, int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            int offset = (page - 1) * pageSize;

            var list = new List<clsorder>();

            string sql = @"
                SELECT id, user_id, address_id, total_amount, status, created_at
                FROM orders
                WHERE user_id = @user_id
                ORDER BY created_at DESC, id DESC
                OFFSET @offset ROWS
                FETCH NEXT @pageSize ROWS ONLY;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new clsorder
                {
                    id = reader.GetInt32("id"),
                    user_id = reader.GetInt32("user_id"),
                    address_id = reader.IsDBNull("address_id") ? (int?)null : reader.GetInt32("address_id"),
                    total_amount = reader.GetDecimal("total_amount"),
                    status = reader.GetString("status"),
                    created_at = reader.GetDateTime("created_at")
                });
            }

            return list;
        }

        // READ - Get all orders (for admin panel) with paging
        public static List<clsorder> GetAllOrders(int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            int offset = (page - 1) * pageSize;

            var list = new List<clsorder>();

            string sql = @"
                SELECT id, user_id, address_id, total_amount, status, created_at
                FROM orders
                ORDER BY created_at DESC, id DESC
                OFFSET @offset ROWS
                FETCH NEXT @pageSize ROWS ONLY;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new clsorder
                {
                    id = reader.GetInt32("id"),
                    user_id = reader.GetInt32("user_id"),
                    address_id = reader.IsDBNull("address_id") ? (int?)null : reader.GetInt32("address_id"),
                    total_amount = reader.GetDecimal("total_amount"),
                    status = reader.GetString("status"),
                    created_at = reader.GetDateTime("created_at")
                });
            }

            return list;
        }

        // UPDATE - Update order status (common for admin)
        public static bool UpdateOrderStatus(int orderId, string newStatus)
        {
            string sql = @"
                UPDATE orders
                SET status = @status
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", orderId);
            cmd.Parameters.AddWithValue("@status", newStatus);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // UPDATE - Full update (rare, but useful for corrections)
        public static bool UpdateOrder(clsorder order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            string sql = @"
                UPDATE orders
                SET user_id = @user_id,
                    address_id = @address_id,
                    total_amount = @total_amount,
                    status = @status
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", order.id);
            cmd.Parameters.AddWithValue("@user_id", order.user_id);
            cmd.Parameters.AddWithValue("@address_id", (object)order.address_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@total_amount", order.total_amount);
            cmd.Parameters.AddWithValue("@status", order.status ?? "pending");

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // DELETE - Cancel order (soft delete or hard - here hard delete, cascades to items & payments)
        public static bool DeleteOrder(int orderId)
        {
            // Note: Due to FK ON DELETE CASCADE, order_items and payments will be deleted automatically
            string sql = "DELETE FROM orders WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", orderId);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // Helper: Get total orders count for a user
        public static int GetOrdersCountByUserId(int userId)
        {
            string sql = "SELECT COUNT(*) FROM orders WHERE user_id = @user_id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@user_id", userId);

            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Helper: Get orders by status (for admin dashboard)
        public static List<clsorder> GetOrdersByStatus(string status, int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            int offset = (page - 1) * pageSize;

            var list = new List<clsorder>();

            string sql = @"
                SELECT id, user_id, address_id, total_amount, status, created_at
                FROM orders
                WHERE status = @status
                ORDER BY created_at DESC
                OFFSET @offset ROWS
                FETCH NEXT @pageSize ROWS ONLY;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new clsorder
                {
                    id = reader.GetInt32("id"),
                    user_id = reader.GetInt32("user_id"),
                    address_id = reader.IsDBNull("address_id") ? (int?)null : reader.GetInt32("address_id"),
                    total_amount = reader.GetDecimal("total_amount"),
                    status = reader.GetString("status"),
                    created_at = reader.GetDateTime("created_at")
                });
            }

            return list;
        }
    }
}