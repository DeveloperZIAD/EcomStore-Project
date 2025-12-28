using Data_layer.Conation;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data_layer
{
    // POCO class for audit_logs
    public class clsauditlog
    {
        public int id { get; set; }
        public string action { get; set; }
        public string details { get; set; }
        public DateTime created_at { get; set; }
    }

    // Data Access Layer for audit_logs
    public static class auditlog_dal
    {
        // CREATE - Add new audit log entry (returns the new log ID)
        public static int AddLog(string action, string details = null)
        {
            string sql = @"
                INSERT INTO audit_logs (action, details)
                VALUES (@action, @details);
                SELECT CAST(SCOPE_IDENTITY() AS int);";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@action", action ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@details", (object)details ?? DBNull.Value);

            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        // CREATE - Add new audit log entry using object
        public static int AddLog(clsauditlog log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            return AddLog(log.action, log.details);
        }

        // READ - Get log by ID
        public static clsauditlog GetLogById(int logId)
        {
            string sql = @"
                SELECT id, action, details, created_at
                FROM audit_logs
                WHERE id = @id;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", logId);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new clsauditlog
                {
                    id = reader.GetInt32("id"),
                    action = reader.GetString("action"),
                    details = reader.IsDBNull("details") ? null : reader.GetString("details"),
                    created_at = reader.GetDateTime("created_at")
                };
            }

            return null; // Not found
        }

        // READ - Get all logs (with optional paging)
        public static List<clsauditlog> GetAllLogs(int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            int offset = (page - 1) * pageSize;

            var list = new List<clsauditlog>();

            string sql = @"
                SELECT id, action, details, created_at
                FROM audit_logs
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
                list.Add(new clsauditlog
                {
                    id = reader.GetInt32("id"),
                    action = reader.GetString("action"),
                    details = reader.IsDBNull("details") ? null : reader.GetString("details"),
                    created_at = reader.GetDateTime("created_at")
                });
            }

            return list;
        }

        // READ - Get logs by action (e.g., 'Guest Order Created')
        public static List<clsauditlog> GetLogsByAction(string action, int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            int offset = (page - 1) * pageSize;

            var list = new List<clsauditlog>();

            string sql = @"
                SELECT id, action, details, created_at
                FROM audit_logs
                WHERE action = @action
                ORDER BY created_at DESC, id DESC
                OFFSET @offset ROWS
                FETCH NEXT @pageSize ROWS ONLY;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@action", action);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new clsauditlog
                {
                    id = reader.GetInt32("id"),
                    action = reader.GetString("action"),
                    details = reader.IsDBNull("details") ? null : reader.GetString("details"),
                    created_at = reader.GetDateTime("created_at")
                });
            }

            return list;
        }

        // READ - Get recent logs (last N entries)
        public static List<clsauditlog> GetRecentLogs(int count = 20)
        {
            if (count < 1) count = 20;
            if (count > 1000) count = 1000; // limit to prevent overload

            var list = new List<clsauditlog>();

            string sql = @"
                SELECT TOP (@count) id, action, details, created_at
                FROM audit_logs
                ORDER BY created_at DESC, id DESC;";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@count", count);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new clsauditlog
                {
                    id = reader.GetInt32("id"),
                    action = reader.GetString("action"),
                    details = reader.IsDBNull("details") ? null : reader.GetString("details"),
                    created_at = reader.GetDateTime("created_at")
                });
            }

            return list;
        }

        // Optional: Delete old logs (e.g., cleanup older than X days) - use carefully!
        public static int DeleteOldLogs(int daysToKeep = 90)
        {
            string sql = @"
                DELETE FROM audit_logs
                WHERE created_at < DATEADD(day, -@days, GETDATE());";

            using var conn = ConnectionManager.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@days", daysToKeep);

            conn.Open();
            return cmd.ExecuteNonQuery();
        }
    }
}