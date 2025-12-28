using Data_layer.Conation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data_layer
{
    // POCO class for Products
    public class clsproduct
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public int stock { get; set; }
        public int? category_id { get; set; }     // Nullable (ON DELETE SET NULL)
        public string image_url { get; set; }
        public DateTime created_at { get; set; }
    }

    // Data Access Layer for Products
    public static partial class productDal
    {
        // CREATE - Add new product (returns the new product ID)
        public static int AddProduct(clsproduct product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            string sql = @"
                INSERT INTO products (name, description, price, stock, category_id, image_url)
                VALUES (@name, @description, @price, @stock, @category_id, @image_url);
                SELECT CAST(SCOPE_IDENTITY() AS int);";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@name", product.name ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@description", (object)product.description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@price", product.price);
            cmd.Parameters.AddWithValue("@stock", product.stock);
            cmd.Parameters.AddWithValue("@category_id", (object)product.category_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@image_url", (object)product.image_url ?? DBNull.Value);

            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        // READ - Get product by ID
        public static clsproduct GetProductById(int productId)
        {
            string sql = @"
                SELECT id, name, description, price, stock, category_id, image_url, created_at
                FROM products
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", productId);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return MapProduct(reader);
            }

            return null; // Not found
        }

        // READ - Get all products with optional paging
        public static List<clsproduct> GetAllProducts(int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            int offset = (page - 1) * pageSize;

            var list = new List<clsproduct>();

            string sql = @"
                SELECT id, name, description, price, stock, category_id, image_url, created_at
                FROM products
                ORDER BY id DESC
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
                list.Add(MapProduct(reader));
            }

            return list;
        }

        // READ - Get products by category ID
        public static List<clsproduct> GetProductsByCategoryId(int categoryId, int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            int offset = (page - 1) * pageSize;

            var list = new List<clsproduct>();

            string sql = @"
                SELECT id, name, description, price, stock, category_id, image_url, created_at
                FROM products
                WHERE category_id = @category_id
                ORDER BY id DESC
                OFFSET @offset ROWS
                FETCH NEXT @pageSize ROWS ONLY;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@category_id", categoryId);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapProduct(reader));
            }

            return list;
        }

        // READ - Search products by name or description (partial match)
        public static List<clsproduct> SearchProducts(string searchTerm, int page = 1, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return GetAllProducts(page, pageSize);

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            int offset = (page - 1) * pageSize;

            var list = new List<clsproduct>();

            string sql = @"
                SELECT id, name, description, price, stock, category_id, image_url, created_at
                FROM products
                WHERE name LIKE @search OR description LIKE @search
                ORDER BY id DESC
                OFFSET @offset ROWS
                FETCH NEXT @pageSize ROWS ONLY;";

            string likeTerm = $"%{searchTerm.Trim()}%";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@search", likeTerm);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapProduct(reader));
            }

            return list;
        }

        // UPDATE - Update an existing product
        public static bool UpdateProduct(clsproduct product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            string sql = @"
                UPDATE products
                SET name = @name,
                    description = @description,
                    price = @price,
                    stock = @stock,
                    category_id = @category_id,
                    image_url = @image_url
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", product.id);
            cmd.Parameters.AddWithValue("@name", product.name ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@description", (object)product.description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@price", product.price);
            cmd.Parameters.AddWithValue("@stock", product.stock);
            cmd.Parameters.AddWithValue("@category_id", (object)product.category_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@image_url", (object)product.image_url ?? DBNull.Value);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // UPDATE - Update stock only (useful after order)
        public static bool UpdateStock(int productId, int newStock)
        {
            if (newStock < 0) return false;

            string sql = "UPDATE products SET stock = @stock WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", productId);
            cmd.Parameters.AddWithValue("@stock", newStock);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // DELETE - Delete product by ID
        public static bool DeleteProduct(int productId)
        {
            string sql = "DELETE FROM products WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", productId);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // Helper: Check current stock
        public static int GetStock(int productId)
        {
            string sql = "SELECT stock FROM products WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", productId);

            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Private helper to map reader to clsproduct
        private static clsproduct MapProduct(SqlDataReader reader)
        {
            return new clsproduct
            {
                id = reader.GetInt32("id"),
                name = reader.GetString("name"),
                description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                price = reader.GetDecimal("price"),
                stock = reader.GetInt32("stock"),
                category_id = reader.IsDBNull("category_id") ? (int?)null : reader.GetInt32("category_id"),
                image_url = reader.IsDBNull("image_url") ? null : reader.GetString("image_url"),
                created_at = reader.GetDateTime("created_at")
            };
        }
    }
}