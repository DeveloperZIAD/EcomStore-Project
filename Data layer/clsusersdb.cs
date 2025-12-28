using Data_layer.Conation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data_layer
{
    // POCO class for Users
    public class clsuser
    {
        public int id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string password_hash { get; set; }   // Nullable for guest users
        public string role { get; set; }            // customer, admin, guest
        public DateTime created_at { get; set; }
    }

    // Data Access Layer for Users
    public static class user_dal
    {
        // CREATE - Add new user (returns the new user ID)
        public static int AddUser(clsuser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            string sql = @"
                INSERT INTO users (username, email, password_hash, role)
                VALUES (@username, @email, @password_hash, @role);
                SELECT CAST(SCOPE_IDENTITY() AS int);";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@username", user.username ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@email", user.email);
            cmd.Parameters.AddWithValue("@password_hash", (object)user.password_hash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@role", user.role ?? "customer");

            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        // READ - Get user by ID
        public static clsuser GetUserById(int userId)
        {
            string sql = @"
                SELECT id, username, email, password_hash, role, created_at
                FROM users
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return MapUser(reader);
            }

            return null; // Not found
        }

        // READ - Get user by email (most common for login)
        public static clsuser GetUserByEmail(string email)
        {
            string sql = @"
                SELECT id, username, email, password_hash, role, created_at
                FROM users
                WHERE email = @email;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", email);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return MapUser(reader);
            }

            return null; // Not found
        }

        // READ - Get user by username
        public static clsuser GetUserByUsername(string username)
        {
            string sql = @"
                SELECT id, username, email, password_hash, role, created_at
                FROM users
                WHERE username = @username;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@username", username);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return MapUser(reader);
            }

            return null;
        }

        // READ - Get all users (for admin panel) with paging
        public static List<clsuser> GetAllUsers(int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            int offset = (page - 1) * pageSize;

            var list = new List<clsuser>();

            string sql = @"
                SELECT id, username, email, password_hash, role, created_at
                FROM users
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
                list.Add(MapUser(reader));
            }

            return list;
        }

        // READ - Get users by role (e.g., all admins or guests)
        public static List<clsuser> GetUsersByRole(string role, int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            int offset = (page - 1) * pageSize;

            var list = new List<clsuser>();

            string sql = @"
                SELECT id, username, email, password_hash, role, created_at
                FROM users
                WHERE role = @role
                ORDER BY created_at DESC
                OFFSET @offset ROWS
                FETCH NEXT @pageSize ROWS ONLY;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@role", role);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(MapUser(reader));
            }

            return list;
        }

        // UPDATE - Update password (for registered users)
        public static bool UpdatePassword(int userId, string newPasswordHash)
        {
            string sql = @"
                UPDATE users
                SET password_hash = @password_hash
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", userId);
            cmd.Parameters.AddWithValue("@password_hash", newPasswordHash);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // UPDATE - Convert guest to customer (set password and change role)
        public static bool ActivateGuestUser(int userId, string username, string passwordHash)
        {
            string sql = @"
                UPDATE users
                SET username = @username,
                    password_hash = @password_hash,
                    role = 'customer'
                WHERE id = @id AND role = 'guest';";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", userId);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password_hash", passwordHash);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // UPDATE - Full update (for admin)
        public static bool UpdateUser(clsuser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            string sql = @"
                UPDATE users
                SET username = @username,
                    email = @email,
                    password_hash = @password_hash,
                    role = @role
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", user.id);
            cmd.Parameters.AddWithValue("@username", user.username ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@email", user.email);
            cmd.Parameters.AddWithValue("@password_hash", (object)user.password_hash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@role", user.role);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // DELETE - Delete user (will cascade to orders, addresses, etc. if configured)
        public static bool DeleteUser(int userId)
        {
            string sql = "DELETE FROM users WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // Helper: Check if email already exists
        public static bool EmailExists(string email)
        {
            string sql = "SELECT COUNT(*) FROM users WHERE email = @email;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", email);

            conn.Open();
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }

        // Helper: Check if username already exists
        public static bool UsernameExists(string username)
        {
            string sql = "SELECT COUNT(*) FROM users WHERE username = @username;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@username", username);

            conn.Open();
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }

        // Private helper to map reader to clsuser
        private static clsuser MapUser(SqlDataReader reader)
        {
            return new clsuser
            {
                id = reader.GetInt32("id"),
                username = reader.IsDBNull("username") ? null : reader.GetString("username"),
                email = reader.GetString("email"),
                password_hash = reader.IsDBNull("password_hash") ? null : reader.GetString("password_hash"),
                role = reader.GetString("role"),
                created_at = reader.GetDateTime("created_at")
            };
        }

    // stored procedure

            public static int CreateUser(
                string username,
                string email,
                string passwordHash,
                string role = "customer")
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentException("Username is required.", nameof(username));

                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email is required.", nameof(email));

                if (string.IsNullOrWhiteSpace(passwordHash))
                    throw new ArgumentException("Password hash is required.", nameof(passwordHash));

                if (!role.Equals("customer", StringComparison.OrdinalIgnoreCase) &&
                    !role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Role must be 'customer' or 'admin'.", nameof(role));

                const string procName = "[dbo].[CreateUser]";

                using var conn = ConnectionManager.GetConnection();
                using var cmd = new SqlCommand(procName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                // Input parameters
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password_hash", passwordHash);
                cmd.Parameters.AddWithValue("@role", role);

                // Output parameter
                var pNewUserId = cmd.Parameters.Add("@new_user_id", SqlDbType.Int);
                pNewUserId.Direction = ParameterDirection.Output;

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    return pNewUserId.Value != DBNull.Value
                        ? Convert.ToInt32(pNewUserId.Value)
                        : -1;
                }
                catch (SqlException ex)
                {
                    // أخطاء التكرار (UNIQUE constraint violation)
                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        if (ex.Message.Contains("username", StringComparison.OrdinalIgnoreCase))
                            throw new InvalidOperationException("Username already exists.");

                        if (ex.Message.Contains("email", StringComparison.OrdinalIgnoreCase))
                            throw new InvalidOperationException("Email already exists.");
                    }

                    // أي خطأ آخر نعيد رميه كما هو
                    throw;
                }
            }
        public static List<clsuser> GetAllUsers()
        {
            var list = new List<clsuser>();

            const string procName = "[dbo].[GetAllUsers]";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(procName, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new clsuser
                    {
                        id = reader.GetInt32("id"),
                        username = reader.IsDBNull("username") ? null : reader.GetString("username"),
                        email = reader.GetString("email"),
                        role = reader.GetString("role"),
                        created_at = reader.GetDateTime("created_at")
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error executing GetAllUsers stored procedure: {ex.Message}", ex);
            }

            return list;
        }
    }
}

