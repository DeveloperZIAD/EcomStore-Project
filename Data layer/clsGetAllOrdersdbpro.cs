using Data_layer.Conation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data_layer
{
    // DTO خاص بعرض تفاصيل الطلبات (لأن الـ SP يرجع بيانات من جداول متعددة)
    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }

        // عنوان الشحن الكامل (للعرض السهل)
        public string FullAddress =>
            string.Join(", ", new[] { Street, City, State, Country, ZipCode }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    public static partial class orderDal
    {
        /// <summary>
        /// جلب جميع الطلبات مع بيانات المستخدم وعنوان الشحن باستخدام الإجراء المخزن [dbo].[GetAllOrders]
        /// مثالي للوحة تحكم الإدارة (Admin Dashboard)
        /// </summary>
        /// <returns>قائمة بملخص الطلبات مرتبة تنازليًا حسب تاريخ الإنشاء</returns>
        public static List<OrderSummaryDto> GetAllOrders()
        {
            var list = new List<OrderSummaryDto>();

            const string procName = "[dbo].[GetAllOrders]";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(procName, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new OrderSummaryDto
                    {
                        OrderId = reader.GetInt32("order_id"),
                        Username = reader.GetString("username"),
                        Email = reader.GetString("email"),
                        TotalAmount = reader.GetDecimal("total_amount"),
                        Status = reader.GetString("status"),
                        CreatedAt = reader.GetDateTime("created_at"),
                        Street = reader.IsDBNull("street") ? null : reader.GetString("street"),
                        City = reader.IsDBNull("city") ? null : reader.GetString("city"),
                        State = reader.IsDBNull("state") ? null : reader.GetString("state"),
                        Country = reader.IsDBNull("country") ? null : reader.GetString("country"),
                        ZipCode = reader.IsDBNull("zip_code") ? null : reader.GetString("zip_code")
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error executing GetAllOrders stored procedure: {ex.Message}", ex);
            }

            return list;
        }
    }
}