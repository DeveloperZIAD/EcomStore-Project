using Data_layer.Conation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data_layer
{
    // POCO class for Categories
    public class clscategory
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime created_at { get; set; }
    }

    // Data Access Layer for Categories
    public static class category_dal
    {
        // CREATE - Add new category (returns the new category ID)
        public static int AddCategory(clscategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            string sql = @"
                INSERT INTO categories (name, description)
                VALUES (@name, @description);
                SELECT CAST(SCOPE_IDENTITY() AS int);";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@name", category.name ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@description", (object)category.description ?? DBNull.Value);

            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        // READ - Get category by ID
        public static clscategory GetCategoryById(int categoryId)
        {
            string sql = @"
                SELECT id, name, description, created_at
                FROM categories
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", categoryId);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new clscategory
                {
                    id = reader.GetInt32("id"),
                    name = reader.GetString("name"),
                    description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                    created_at = reader.GetDateTime("created_at")
                };
            }

            return null; // Not found
        }

        // READ - Get category by name (useful for lookup)
        public static clscategory GetCategoryByName(string name)
        {
            string sql = @"
                SELECT id, name, description, created_at
                FROM categories
                WHERE name = @name;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", name);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new clscategory
                {
                    id = reader.GetInt32("id"),
                    name = reader.GetString("name"),
                    description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                    created_at = reader.GetDateTime("created_at")
                };
            }

            return null;
        }

        // READ - Get all categories
        public static List<clscategory> GetAllCategories()
        {
            var list = new List<clscategory>();

            string sql = @"
                SELECT id, name, description, created_at
                FROM categories
                ORDER BY name ASC;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new clscategory
                {
                    id = reader.GetInt32("id"),
                    name = reader.GetString("name"),
                    description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                    created_at = reader.GetDateTime("created_at")
                });
            }

            return list;
        }

        // UPDATE - Update an existing category
        public static bool UpdateCategory(clscategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            string sql = @"
                UPDATE categories
                SET name = @name,
                    description = @description
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", category.id);
            cmd.Parameters.AddWithValue("@name", category.name ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@description", (object)category.description ?? DBNull.Value);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // DELETE - Delete category by ID
        // Warning: This will fail if there are products linked (due to FK constraint)
        public static bool DeleteCategory(int categoryId)
        {
            string sql = "DELETE FROM categories WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", categoryId);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // Helper: Check if category has products (to avoid deletion errors)
        public static bool HasProducts(int categoryId)
        {
            string sql = "SELECT COUNT(*) FROM products WHERE category_id = @category_id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@category_id", categoryId);

            conn.Open();
            object result = cmd.ExecuteScalar();
            int count = result != null ? Convert.ToInt32(result) : 0;
            return count > 0;
        }


        // procedure ----------------------------------------


        public static List<clscategory> GetAllCategoriesPro()
        {
            var list = new List<clscategory>();

            const string procName = "[dbo].[GetAllCategories]";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(procName, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new clscategory
                    {
                        id = reader.GetInt32("id"),
                        name = reader.GetString("name"),
                        description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                        created_at = reader.GetDateTime("created_at")
                    });
                }
            }
            catch (SqlException ex)
            {
                // يمكنك تسجيل الخطأ هنا أو إعادة رميه برسالة أوضح
                throw new Exception($"Error executing GetAllCategories stored procedure: {ex.Message}", ex);
            }

            return list;
        }
    

        public static int CreateCategoryProc(string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name is required.", nameof(name));

            string procName = "[dbo].[CreateCategory]";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(procName, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@name", name);
            if (description != null)
                cmd.Parameters.AddWithValue("@description", description);
            else
                cmd.Parameters.AddWithValue("@description", DBNull.Value);

            conn.Open();

            // Since the proc does not return the ID, we need to get it separately
            // Best way: use OUTPUT parameter or SCOPE_IDENTITY after insert
            // But your current proc doesn't return ID, so we'll query it by unique name
            // (since name is UNIQUE in the table)

            cmd.ExecuteNonQuery();

            // Get the newly created category ID by name
            string sql = "SELECT id FROM categories WHERE name = @name;";
            using var cmdId = new SqlCommand(sql, conn);
            cmdId.Parameters.AddWithValue("@name", name);

            object result = cmdId.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }


        public static int CreateCategoryProc_WithOutput(string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name is required.", nameof(name));

            string procName = "[dbo].[CreateCategory]";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(procName, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@description", (object)description ?? DBNull.Value);

            var outputId = cmd.Parameters.Add("@new_id", System.Data.SqlDbType.Int);
            outputId.Direction = System.Data.ParameterDirection.Output;

            conn.Open();
            cmd.ExecuteNonQuery();

            return outputId.Value != DBNull.Value ? Convert.ToInt32(outputId.Value) : -1;

        }
    }
}
    
