using Data_layer.Conation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data_layer
{
    // DTO لتفاصيل الطلب الرئيسية (من الاستعلام الأول)
    public class OrderDetailsDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? AddressId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // من جدول users
        public string Username { get; set; }
        public string Email { get; set; }

        // من جدول addresses
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    // نستخدم الكلاسات الموجودة مسبقًا للعناصر والدفع
    // clsorderitem و clspayment (افتراضًا أنها موجودة في المشروع)

    // نتيجة شاملة لتفاصيل الطلب
    public class FullOrderDetails
    {
        public OrderDetailsDto Order { get; set; }
        public List<clsorderitem> Items { get; set; } = new List<clsorderitem>();
        public clspayment Payment { get; set; }
    }

    public static partial class orderDal
    {
        /// <summary>
        /// جلب تفاصيل طلب كاملة (الطلب + العناصر + الدفعة) باستخدام الإجراء المخزن [dbo].[GetOrderDetails]
        /// مثالي لصفحة تفاصيل الطلب للعميل أو الإدارة
        /// </summary>
        /// <param name="orderId">معرف الطلب</param>
        /// <returns>كائن يحتوي على كل تفاصيل الطلب، أو null إذا لم يوجد</returns>
        public static FullOrderDetails GetOrderDetails(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Order ID must be greater than zero.", nameof(orderId));

            const string procName = "[dbo].[GetOrderDetails]";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(procName, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@order_id", orderId);

            try
            {
                conn.Open();
                using var reader = cmd.ExecuteReader();

                FullOrderDetails result = null;

                // أول Result Set: تفاصيل الطلب الرئيسية
                if (reader.Read())
                {
                    result = new FullOrderDetails
                    {
                        Order = new OrderDetailsDto
                        {
                            Id = reader.GetInt32("id"),
                            UserId = reader.GetInt32("user_id"),
                            AddressId = reader.IsDBNull("address_id") ? (int?)null : reader.GetInt32("address_id"),
                            TotalAmount = reader.GetDecimal("total_amount"),
                            Status = reader.GetString("status"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            Username = reader.GetString("username"),
                            Email = reader.GetString("email"),
                            Street = reader.GetString("street"),
                            City = reader.GetString("city"),
                            Country = reader.GetString("country")
                        }
                    };
                }
                else
                {
                    return null; // الطلب غير موجود
                }

                // ثاني Result Set: عناصر الطلب
                reader.NextResult();
                while (reader.Read())
                {
                    result.Items.Add(new clsorderitem
                    {
                        id = reader.GetInt32("id"),
                        order_id = reader.GetInt32("order_id"),
                        product_id = reader.GetInt32("product_id"),
                        quantity = reader.GetInt32("quantity"),
                        price_at_purchase = reader.GetDecimal("price_at_purchase")
                    });
                }

                // ثالث Result Set: الدفعة
                reader.NextResult();
                if (reader.Read())
                {
                    result.Payment = new clspayment
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

                return result;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error executing GetOrderDetails stored procedure: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// Gets the payment for a specific order (assuming one payment per order)
        /// </summary>
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

            return null; // No payment found
        }
    }
}