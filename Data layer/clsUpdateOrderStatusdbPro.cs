using System;
using Microsoft.Data.SqlClient;
using System.Data;
using Data_layer.Conation;

namespace Data_layer
{
    public static partial class orderDal
    {
        /// <summary>
        /// تحديث حالة الطلب باستخدام الإجراء المخزن [dbo].[UpdateOrderStatus]
        /// مثالي لتغيير الحالة من pending → processing → shipped → delivered أو cancelled
        /// </summary>
        /// <param name="orderId">معرف الطلب</param>
        /// <param name="newStatus">الحالة الجديدة (pending, processing, shipped, delivered, cancelled)</param>
        /// <returns>true إذا تم التحديث بنجاح، false إذا الطلب غير موجود</returns>
        public static bool UpdateOrderStatusPro(int orderId, string newStatus)
        {
            if (orderId <= 0)
                throw new ArgumentException("Order ID must be greater than zero.", nameof(orderId));

            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("New status is required.", nameof(newStatus));

            // اختياري: تحقق من أن الحالة الجديدة من القيم المسموحة
            var validStatuses = new[] { "pending", "processing", "shipped", "delivered", "cancelled" };
            if (Array.IndexOf(validStatuses, newStatus.ToLower()) == -1)
                throw new ArgumentException("Invalid order status. Allowed values: pending, processing, shipped, delivered, cancelled.", nameof(newStatus));

            const string procName = "[dbo].[UpdateOrderStatus]";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(procName, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@order_id", orderId);
            cmd.Parameters.AddWithValue("@new_status", newStatus);

            try
            {
                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0; // true إذا تم تحديث صف واحد (الطلب موجود)
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error executing UpdateOrderStatus stored procedure: {ex.Message}", ex);
            }
        }
    }
}