using System;
using Microsoft.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;  // أو System.Text.Json حسب اللي بتستخدمه
using System.Collections.Generic;
using Data_layer.Conation;

namespace Data_layer
{
    // DTO لعنصر الطلب اللي هيتبعت في الـ JSON
    public class OrderItemDto
    {
        public int product_id { get; set; }
        public int quantity { get; set; }
        public decimal price_at_purchase { get; set; }
    }

    // نتيجة الإجراء المخزن (للرجوع للتطبيق)
    public class CreateGuestOrderResult
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    // طبقة منفصلة لعمليات الـ Guest Checkout
    public static class guestcheckout_dal
    {
        /// <summary>
        /// تنفيذ الإجراء المخزن CreateGuestOrder
        /// يقوم بإنشاء guest user (أو استخدام موجود)، عنوان، طلب، عناصر الطلب، دفعة، وتحديث المخزون
        /// </summary>
        /// <param name="email">بريد العميل (إجباري وفريد)</param>
        /// <param name="username">اسم المستخدم الاختياري (لو مش موجود هيستخدم الإيميل)</param>
        /// <param name="street">الشارع</param>
        /// <param name="city">المدينة</param>
        /// <param name="state">الولاية (اختياري)</param>
        /// <param name="country">الدولة</param>
        /// <param name="zip_code">الرمز البريدي</param>
        /// <param name="orderItems">قائمة عناصر الطلب</param>
        /// <param name="payment_method">طريقة الدفع (credit_card, paypal, cash_on_delivery)</param>
        /// <param name="payment_status">حالة الدفع (pending أو completed بعد نجاح البوابة)</param>
        /// <param name="transaction_id">رقم المعاملة من بوابة الدفع (اختياري)</param>
        /// <returns>كائن يحتوي على OrderId و UserId وحالة النجاح</returns>
        public static CreateGuestOrderResult CreateGuestOrder(
            string email,
            string username,
            string street,
            string city,
            string state,
            string country,
            string zip_code,
            List<OrderItemDto> orderItems,
            string payment_method,
            string payment_status = "pending",
            string transaction_id = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            if (orderItems == null || orderItems.Count == 0)
                throw new ArgumentException("Order items cannot be empty.", nameof(orderItems));

            // تحويل قائمة العناصر إلى JSON
            string orderItemsJson = JsonConvert.SerializeObject(orderItems);

            string procName = "[dbo].[CreateGuestOrder]";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(procName, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // Input parameters
            cmd.Parameters.AddWithValue("@email", email);
            if (!string.IsNullOrWhiteSpace(username))
                cmd.Parameters.AddWithValue("@username", username);
            else
                cmd.Parameters.AddWithValue("@username", DBNull.Value);

            cmd.Parameters.AddWithValue("@street", street ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@city", city ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@state", (object)state ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@country", country ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@zip_code", zip_code ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue("@order_items_json", orderItemsJson);
            cmd.Parameters.AddWithValue("@payment_method", payment_method);
            cmd.Parameters.AddWithValue("@payment_status", payment_status);

            if (!string.IsNullOrWhiteSpace(transaction_id))
                cmd.Parameters.AddWithValue("@transaction_id", transaction_id);
            else
                cmd.Parameters.AddWithValue("@transaction_id", DBNull.Value);

            // Output parameters
            var pOrderId = cmd.Parameters.Add("@new_order_id", SqlDbType.Int);
            pOrderId.Direction = ParameterDirection.Output;

            var pUserId = cmd.Parameters.Add("@new_user_id", SqlDbType.Int);
            pUserId.Direction = ParameterDirection.Output;

            var result = new CreateGuestOrderResult();

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();

                result.OrderId = pOrderId.Value != DBNull.Value ? Convert.ToInt32(pOrderId.Value) : 0;
                result.UserId = pUserId.Value != DBNull.Value ? Convert.ToInt32(pUserId.Value) : 0;
                result.Success = true;
            }
            catch (SqlException ex)
            {
                // أخطاء مخصصة من الـ SP (مثل insufficient stock أو admin email)
                result.Success = false;
                result.ErrorMessage = ex.Message;

                // يمكنك تحليل ex.Number لرسائل أدق
                // 50001 = admin email, 50002 = product not found, 50003 = insufficient stock
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }
    }
}