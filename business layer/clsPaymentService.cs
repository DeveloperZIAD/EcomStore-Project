// File: PaymentService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Data_layer;
using Business_layer.Dtos;

namespace Business_layer
{
    public static class PaymentService
    {
        /// <summary>
        /// Adds a new payment record (typically after order creation)
        /// </summary>
        public static int AddPayment(int orderId, decimal amount, string paymentMethod, string status = "pending", string transactionId = null)
        {
            if (orderId <= 0) throw new ArgumentException("Invalid order ID.");
            if (amount < 0) throw new ArgumentException("Amount cannot be negative.");
            if (string.IsNullOrWhiteSpace(paymentMethod)) throw new ArgumentException("Payment method is required.");

            var validMethods = new[] { "credit_card", "paypal", "cash_on_delivery" };
            if (!validMethods.Contains(paymentMethod.ToLower()))
                throw new ArgumentException("Invalid payment method.");

            var payment = new clspayment
            {
                order_id = orderId,
                amount = amount,
                payment_method = paymentMethod,
                status = status,
                transaction_id = transactionId
            };

            int newPaymentId = paymentDal.AddPayment(payment);

            if (newPaymentId <= 0)
                throw new Exception("Failed to add the payment.");

            AuditLogService.LogAction("Payment Created", $"Payment ID: {newPaymentId}, Order ID: {orderId}, Amount: {amount}, Method: {paymentMethod}");

            return newPaymentId;
        }

        /// <summary>
        /// Retrieves payment details by payment ID
        /// </summary>
        public static PaymentResponseDto GetPaymentById(int paymentId)
        {
            if (paymentId <= 0) throw new ArgumentException("Invalid payment ID.");

            var payment = paymentDal.GetPaymentById(paymentId);

            if (payment == null)
                throw new KeyNotFoundException("Payment not found.");

            return MapToResponseDto(payment);
        }

        /// <summary>
        /// Retrieves payment details by order ID (assuming one payment per order)
        /// </summary>
        public static PaymentResponseDto GetPaymentByOrderId(int orderId)
        {
            if (orderId <= 0) throw new ArgumentException("Invalid order ID.");

            var payment = paymentDal.GetPaymentByOrderId(orderId);

            if (payment == null)
                throw new KeyNotFoundException("Payment not found for this order.");

            return MapToResponseDto(payment);
        }

        /// <summary>
        /// Retrieves all payments (admin view)
        /// </summary>
        public static List<PaymentResponseDto> GetAllPayments()
        {
            var dbPayments = paymentDal.GetAllPayments();

            return dbPayments.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Updates payment status (commonly used after payment gateway callback)
        /// </summary>
        public static bool UpdatePaymentStatus(int paymentId, string newStatus, string transactionId = null)
        {
            if (paymentId <= 0)
                throw new ArgumentException("Invalid payment ID.");

            ValidateStatus(newStatus);

            var existing = paymentDal.GetPaymentById(paymentId);

            if (existing == null)
                throw new KeyNotFoundException("Payment not found.");

            // مقارنة الحالة بدون مراعاة كبير/صغير (OrdinalIgnoreCase)
            bool statusChanged = !existing.status.Equals(newStatus, StringComparison.OrdinalIgnoreCase);

            bool success = paymentDal.UpdatePaymentStatus(paymentId, newStatus, transactionId);

            if (success && statusChanged)
            {
                AuditLogService.LogAction("Payment Status Updated",
                    $"Payment ID: {paymentId}, Order ID: {existing.order_id}, New Status: {newStatus}, Transaction ID: {transactionId ?? "N/A"}");
            }

            return success;
        }        /// <summary>
                 /// Updates payment status using order ID (useful for webhooks that only provide order reference)
                 /// </summary>
        public static bool UpdatePaymentStatusByOrderId(int orderId, string newStatus, string transactionId = null)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID.");

            ValidateStatus(newStatus);

            var existing = paymentDal.GetPaymentByOrderId(orderId);

            if (existing == null)
                throw new KeyNotFoundException("Payment not found for this order.");

            bool statusChanged = !existing.status.Equals(newStatus, StringComparison.OrdinalIgnoreCase);

            bool success = paymentDal.UpdatePaymentStatusByOrderId(orderId, newStatus, transactionId);

            if (success && statusChanged)
            {
                AuditLogService.LogAction("Payment Status Updated (by Order)",
                    $"Order ID: {orderId}, Payment ID: {existing.id}, New Status: {newStatus}, Transaction ID: {transactionId ?? "N/A"}");
            }

            return success;
        }
        // ----------------- Private Helper Methods -----------------

        private static void ValidateUpdateDto(UpdatePaymentStatusRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.NewStatus))
                throw new ArgumentException("New status is required.");

            var validStatuses = new[] { "pending", "completed", "failed" };
            if (!validStatuses.Contains(dto.NewStatus.ToLower()))
                throw new ArgumentException("Invalid payment status. Allowed: pending, completed, failed.");
        }
        private static void ValidateStatus(string status)
        {
            var validStatuses = new[] { "pending", "completed", "failed" };
            if (string.IsNullOrWhiteSpace(status) || !validStatuses.Contains(status.ToLower()))
                throw new ArgumentException("Invalid payment status. Allowed: pending, completed, failed.");
        }

        private static PaymentResponseDto MapToResponseDto(clspayment db)
        {
            return new PaymentResponseDto
            {
                Id = db.id,
                OrderId = db.order_id,
                Amount = db.amount,
                PaymentMethod = db.payment_method,
                Status = db.status,
                TransactionId = db.transaction_id,
                CreatedAt = db.created_at
            };
        }
    }
}