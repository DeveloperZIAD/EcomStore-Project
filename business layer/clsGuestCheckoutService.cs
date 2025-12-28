// File: Business_layer/GuestCheckoutService.cs
using System;
using System.Collections.Generic;
using Data_layer;

namespace Business_layer
{
    public static class GuestCheckoutService
    {
        /// <summary>
        /// Creates a complete guest order
        /// Calls the stored procedure CreateGuestOrder
        /// </summary>
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
            // Validation أساسية (الـ DAL بيعمل validation أكتر، لكن نضيف شوية هنا)
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            if (orderItems == null || orderItems.Count == 0)
                throw new ArgumentException("Order items cannot be empty.");

            if (string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(country) || string.IsNullOrWhiteSpace(zip_code))
                throw new ArgumentException("Shipping address fields are required.");

            if (string.IsNullOrWhiteSpace(payment_method))
                throw new ArgumentException("Payment method is required.");

            // ندعو الـ DAL مباشرة
            var result = guestcheckout_dal.CreateGuestOrder(
                email: email.Trim(),
                username: string.IsNullOrWhiteSpace(username) ? null : username.Trim(),
                street: street.Trim(),
                city: city.Trim(),
                state: string.IsNullOrWhiteSpace(state) ? null : state.Trim(),
                country: country.Trim(),
                zip_code: zip_code.Trim(),
                orderItems: orderItems,
                payment_method: payment_method,
                payment_status: payment_status,
                transaction_id: transaction_id
            );

            // لو نجح، نسجل audit log
            if (result.Success)
            {
                AuditLogService.LogAction("Guest Order Created",
                    $"Order ID: {result.OrderId}, User ID: {result.UserId}, Email: {email}");
            }

            return result;
        }
    }
}