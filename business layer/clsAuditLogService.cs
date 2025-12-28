// File: AuditLogService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Data_layer;
using Business_layer.Dtos;

namespace Business_layer
{
    public static class AuditLogService
    {
        /// <summary>
        /// Retrieves all audit logs ordered by creation date (newest first)
        /// </summary>
        public static List<AuditLogResponseDto> GetAllLogs()
        {
            var dbLogs = auditlog_dal.GetAllLogs();

            return dbLogs.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Retrieves recent audit logs (default: last 50 entries)
        /// </summary>
        public static List<AuditLogResponseDto> GetRecentLogs(int count = 50)
        {
            if (count <= 0) count = 50;
            if (count > 500) count = 500; // limit to prevent overload

            var dbLogs = auditlog_dal.GetRecentLogs(count);

            return dbLogs.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Retrieves audit logs filtered by a specific action
        /// </summary>
        public static List<AuditLogResponseDto> GetLogsByAction(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action cannot be empty.", nameof(action));

            var dbLogs = auditlog_dal.GetLogsByAction(action.Trim());

            return dbLogs.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Adds a new audit log entry (typically called internally by other services)
        /// </summary>
        public static int LogAction(string action, string details = null)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action is required.", nameof(action));

            return auditlog_dal.AddLog(action.Trim(), details?.Trim());
        }

        // ----------------- Private Helper Method -----------------

        private static AuditLogResponseDto MapToResponseDto(clsauditlog db)
        {
            return new AuditLogResponseDto
            {
                Id = db.id,
                Action = db.action,
                Details = db.details,
                CreatedAt = db.created_at
            };
        }
    }
}