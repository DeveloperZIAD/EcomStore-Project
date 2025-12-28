using Data_layer.Conation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data_layer
{
    // DTO خاص بعرض تفاصيل المنتجات (لأن الـ SP يرجع بيانات من جدولين: products + categories)
    public class ProductSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string CategoryName { get; set; }     // اسم الفئة (قد يكون null إذا لا فئة)
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public static partial class productDal
    {
        /// <summary>
        /// جلب جميع المنتجات مع اسم الفئة باستخدام الإجراء المخزن [dbo].[GetAllProducts]
        /// مثالي لعرض كتالوج المنتجات في المتجر أو لوحة الإدارة
        /// </summary>
        /// <returns>قائمة بملخص المنتجات مرتبة حسب الـ ID</returns>
        public static List<ProductSummaryDto> GetAllProducts()
        {
            var list = new List<ProductSummaryDto>();

            const string procName = "[dbo].[GetAllProducts]";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(procName, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new ProductSummaryDto
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                        Price = reader.GetDecimal("price"),
                        Stock = reader.GetInt32("stock"),
                        CategoryName = reader.IsDBNull("category_name") ? null : reader.GetString("category_name"),
                        ImageUrl = reader.IsDBNull("image_url") ? null : reader.GetString("image_url"),
                        CreatedAt = reader.GetDateTime("created_at")
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error executing GetAllProducts stored procedure: {ex.Message}", ex);
            }

            return list;
        }
    }
}