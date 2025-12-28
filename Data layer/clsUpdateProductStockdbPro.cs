using System;
using Microsoft.Data.SqlClient;
using System.Data;
using Data_layer.Conation;

namespace Data_layer
{
    public static partial class productDal
    {
        /// <summary>
        /// تحديث كمية المخزون لمنتج معين باستخدام الإجراء المخزن [dbo].[UpdateProductStock]
        /// مثالي للإدارة اليدوية للمخزون أو تصحيح الأخطاء أو إعادة التخزين
        /// </summary>
        /// <param name="productId">معرف المنتج</param>
        /// <param name="newStock">الكمية الجديدة للمخزون (يجب أن تكون >= 0)</param>
        /// <returns>true إذا تم التحديث بنجاح، false إذا المنتج غير موجود</returns>
        public static bool UpdateProductStockPro(int productId, int newStock)
        {
            if (productId <= 0)
                throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));

            if (newStock < 0)
                throw new ArgumentException("New stock cannot be negative.", nameof(newStock));

            const string procName = "[dbo].[UpdateProductStock]";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(procName, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@product_id", productId);
            cmd.Parameters.AddWithValue("@new_stock", newStock);

            try
            {
                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0; // true إذا تم تحديث صف واحد (المنتج موجود)
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error executing UpdateProductStock stored procedure: {ex.Message}", ex);
            }
        }
    }
}