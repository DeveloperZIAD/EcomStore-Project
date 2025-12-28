using Data_layer.Conation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data_layer
{
    // POCO class for Payments
    public class clspayment
    {
        public int id { get; set; }
        public int order_id { get; set; }
        public decimal amount { get; set; }
        public string payment_method { get; set; }   // credit_card, paypal, cash_on_delivery
        public string status { get; set; }            // pending, completed, failed
        public string transaction_id { get; set; }   // Optional external transaction reference
        public DateTime created_at { get; set; }
    }

    // Data Access Layer for Payments
    public static partial class paymentDal
    {
        // CREATE - Add new payment (returns the new payment ID)
        public static int AddPayment(clspayment payment)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            string sql = @"
                INSERT INTO payments (order_id, amount, payment_method, status, transaction_id)
                VALUES (@order_id, @amount, @payment_method, @status, @transaction_id);
                SELECT CAST(SCOPE_IDENTITY() AS int);";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@order_id", payment.order_id);
            cmd.Parameters.AddWithValue("@amount", payment.amount);
            cmd.Parameters.AddWithValue("@payment_method", payment.payment_method);
            cmd.Parameters.AddWithValue("@status", payment.status ?? "pending");
            cmd.Parameters.AddWithValue("@transaction_id", (object)payment.transaction_id ?? DBNull.Value);

            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        // READ - Get payment by ID
        public static clspayment GetPaymentById(int paymentId)
        {
            string sql = @"
                SELECT id, order_id, amount, payment_method, status, transaction_id, created_at
                FROM payments
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", paymentId);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new clspayment
                {
                    id = reader.GetInt32("id"),
                    order_id = reader.GetInt32("order_id"),
                    amount = reader.GetDecimal("amount"),
                    payment_method = reader.GetString("payment_method"),
                    status = reader.GetString("status"),
                    transaction_id = reader.IsDBNull("transaction_id") ? null : reader.GetString("transaction_id"),
                    created_at = reader.GetDateTime("created_at")
                };
            }

            return null; // Not found
        }

        // READ - Get payment by order ID (assuming one payment per order)
        public static clspayment GetPaymentByOrderId(int orderId)
        {
            string sql = @"
                SELECT id, order_id, amount, payment_method, status, transaction_id, created_at
                FROM payments
                WHERE order_id = @order_id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@order_id", orderId);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new clspayment
                {
                    id = reader.GetInt32("id"),
                    order_id = reader.GetInt32("order_id"),
                    amount = reader.GetDecimal("amount"),
                    payment_method = reader.GetString("payment_method"),
                    status = reader.GetString("status"),
                    transaction_id = reader.IsDBNull("transaction_id") ? null : reader.GetString("transaction_id"),
                    created_at = reader.GetDateTime("created_at")
                };
            }

            return null; // No payment found for this order
        }

        // READ - Get all payments (for admin) with paging
        public static List<clspayment> GetAllPayments(int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            int offset = (page - 1) * pageSize;

            var list = new List<clspayment>();

            string sql = @"
                SELECT id, order_id, amount, payment_method, status, transaction_id, created_at
                FROM payments
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
                list.Add(new clspayment
                {
                    id = reader.GetInt32("id"),
                    order_id = reader.GetInt32("order_id"),
                    amount = reader.GetDecimal("amount"),
                    payment_method = reader.GetString("payment_method"),
                    status = reader.GetString("status"),
                    transaction_id = reader.IsDBNull("transaction_id") ? null : reader.GetString("transaction_id"),
                    created_at = reader.GetDateTime("created_at")
                });
            }

            return list;
        }

        // UPDATE - Update payment status (common after gateway callback)
        public static bool UpdatePaymentStatus(int paymentId, string newStatus, string transactionId = null)
        {
            string sql = @"
                UPDATE payments
                SET status = @status"
                + (transactionId != null ? ", transaction_id = @transaction_id" : "")
                + @" WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", paymentId);
            cmd.Parameters.AddWithValue("@status", newStatus);
            if (transactionId != null)
                cmd.Parameters.AddWithValue("@transaction_id", transactionId);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // UPDATE - Update payment by order ID (useful when you only have order_id)
        public static bool UpdatePaymentStatusByOrderId(int orderId, string newStatus, string transactionId = null)
        {
            string sql = @"
                UPDATE payments
                SET status = @status"
                + (transactionId != null ? ", transaction_id = @transaction_id" : "")
                + @" WHERE order_id = @order_id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@order_id", orderId);
            cmd.Parameters.AddWithValue("@status", newStatus);
            if (transactionId != null)
                cmd.Parameters.AddWithValue("@transaction_id", transactionId);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // UPDATE - Full update (rare, for admin corrections)
        public static bool UpdatePayment(clspayment payment)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            string sql = @"
                UPDATE payments
                SET order_id = @order_id,
                    amount = @amount,
                    payment_method = @payment_method,
                    status = @status,
                    transaction_id = @transaction_id
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", payment.id);
            cmd.Parameters.AddWithValue("@order_id", payment.order_id);
            cmd.Parameters.AddWithValue("@amount", payment.amount);
            cmd.Parameters.AddWithValue("@payment_method", payment.payment_method);
            cmd.Parameters.AddWithValue("@status", payment.status);
            cmd.Parameters.AddWithValue("@transaction_id", (object)payment.transaction_id ?? DBNull.Value);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // Helper: Get payments by status (for admin dashboard)
        public static List<clspayment> GetPaymentsByStatus(string status, int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            int offset = (page - 1) * pageSize;

            var list = new List<clspayment>();

            string sql = @"
                SELECT id, order_id, amount, payment_method, status, transaction_id, created_at
                FROM payments
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
                list.Add(new clspayment
                {
                    id = reader.GetInt32("id"),
                    order_id = reader.GetInt32("order_id"),
                    amount = reader.GetDecimal("amount"),
                    payment_method = reader.GetString("payment_method"),
                    status = reader.GetString("status"),
                    transaction_id = reader.IsDBNull("transaction_id") ? null : reader.GetString("transaction_id"),
                    created_at = reader.GetDateTime("created_at")
                });
            }

            return list;
        }
    }
}