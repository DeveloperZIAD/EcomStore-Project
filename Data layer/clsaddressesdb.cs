using Data_layer.Conation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Linq;

namespace Data_layer
{
    // Fixed POCO class for Addresses
    public class clsaddressesdb
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }         // Can be null
        public string zip_code { get; set; }
        public string country { get; set; }
        public bool is_default { get; set; }      // Fixed: bool instead of BitArray
    }

    // Data Access Layer for Addresses
    public static class addressesdb_dal
    {
        // CREATE - Add new address (returns the new address ID)
        public static int AddAddress(clsaddressesdb address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            string sql = @"
                INSERT INTO addresses (user_id, street, city, state, country, zip_code, is_default)
                VALUES (@user_id, @street, @city, @state, @country, @zip_code, @is_default);
                SELECT CAST(SCOPE_IDENTITY() AS int);";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@user_id", address.user_id);
            cmd.Parameters.AddWithValue("@street", address.street ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@city", address.city ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@state", (object)address.state ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@country", address.country ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@zip_code", address.zip_code ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@is_default", address.is_default);

            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        // READ - Get address by ID
        public static clsaddressesdb GetAddressById(int addressId)
        {
            string sql = @"
                SELECT id, user_id, street, city, state, country, zip_code, is_default
                FROM addresses
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", addressId);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new clsaddressesdb
                {
                    id = reader.GetInt32("id"),
                    user_id = reader.GetInt32("user_id"),
                    street = reader.GetString("street"),
                    city = reader.GetString("city"),
                    state = reader.IsDBNull("state") ? null : reader.GetString("state"),
                    country = reader.GetString("country"),
                    zip_code = reader.GetString("zip_code"),
                    is_default = reader.GetBoolean("is_default")
                };
            }

            return null; // Not found
        }

        // READ - Get all addresses for a specific user
        public static List<clsaddressesdb> GetAddressesByUserId(int userId)
        {
            var list = new List<clsaddressesdb>();

            string sql = @"
                SELECT id, user_id, street, city, state, country, zip_code, is_default
                FROM addresses
                WHERE user_id = @user_id
                ORDER BY is_default DESC, id DESC;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@user_id", userId);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new clsaddressesdb
                {
                    id = reader.GetInt32("id"),
                    user_id = reader.GetInt32("user_id"),
                    street = reader.GetString("street"),
                    city = reader.GetString("city"),
                    state = reader.IsDBNull("state") ? null : reader.GetString("state"),
                    country = reader.GetString("country"),
                    zip_code = reader.GetString("zip_code"),
                    is_default = reader.GetBoolean("is_default")
                });
            }

            return list;
        }

        // UPDATE - Update an existing address
        public static bool UpdateAddress(clsaddressesdb address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));

            string sql = @"
                UPDATE addresses
                SET user_id = @user_id,
                    street = @street,
                    city = @city,
                    state = @state,
                    country = @country,
                    zip_code = @zip_code,
                    is_default = @is_default
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", address.id);
            cmd.Parameters.AddWithValue("@user_id", address.user_id);
            cmd.Parameters.AddWithValue("@street", address.street ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@city", address.city ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@state", (object)address.state ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@country", address.country ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@zip_code", address.zip_code ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@is_default", address.is_default);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // DELETE - Delete address by ID
        public static bool DeleteAddress(int addressId)
        {
            string sql = "DELETE FROM addresses WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", addressId);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        // Helper: Set an address as default for a user (unsets others)
        public static bool SetDefaultAddress(int addressId, int userId)
        {
            using var conn = ConnectionManager.GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // First, unset all addresses for this user
                string sqlUnset = "UPDATE addresses SET is_default = 0 WHERE user_id = @user_id;";
                using var cmdUnset = new SqlCommand(sqlUnset, conn, transaction);
                cmdUnset.Parameters.AddWithValue("@user_id", userId);
                cmdUnset.ExecuteNonQuery();

                // Then, set the selected one as default
                string sqlSet = "UPDATE addresses SET is_default = 1 WHERE id = @id AND user_id = @user_id;";
                using var cmdSet = new SqlCommand(sqlSet, conn, transaction);
                cmdSet.Parameters.AddWithValue("@id", addressId);
                cmdSet.Parameters.AddWithValue("@user_id", userId);

                int rows = cmdSet.ExecuteNonQuery();
                transaction.Commit();

                return rows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}