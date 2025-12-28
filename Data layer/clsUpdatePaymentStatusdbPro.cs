using System;
using Microsoft.Data.SqlClient;
using System.Data;
using Data_layer.Conation;

namespace Data_layer
{
    public static partial class paymentDal
    {
        /// <summary>
        /// تحديث حالة الدفعة باستخدام الإجراء المخزن [dbo].[UpdatePaymentStatus]
        /// مثالي لتحديث الحالة بعد رد من بوابة الدفع (مثل pending → completed أو failed)
        /// </summary>
        /// <param name="paymentId">معرف الدفعة</param>
        /// <param name="newStatus">الحالة الجديدة (pending, completed, failed)</param>
        /// <returns>true إذا تم التحديث بنجاح، false إذا الدفعة غير موجودة</returns>
        public static bool UpdatePaymentStatusPro(int paymentId, string newStatus)
        {
            if (paymentId <= 0)
                throw new ArgumentException("Payment ID must be greater than zero.", nameof(paymentId));

            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("New status is required.", nameof(newStatus));

            // اختياري: تحقق من أن الحالة الجديدة مسموح بها
            var validStatuses = new[] { "pending", "completed", "failed" };
            if (Array.IndexOf(validStatuses, newStatus.ToLower()) == -1)
                throw new ArgumentException("Invalid payment status. Allowed values: pending, completed, failed.", nameof(newStatus));

            const string procName = "[dbo].[UpdatePaymentStatus]";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(procName, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@payment_id", paymentId);
            cmd.Parameters.AddWithValue("@new_status", newStatus);

            try
            {
                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0; // true إذا تم تحديث صف واحد
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error executing UpdatePaymentStatus stored procedure: {ex.Message}", ex);
            }
        }
    }
}