using Business_layer;
using Data_layer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuestCheckoutController : ControllerBase
    {
        /// <summary>
        /// Creates a complete order for a guest user (no login required)
        /// </summary>
        [HttpPost]
        [AllowAnonymous] // مفتوح للجميع - ده الـ Guest Checkout
        public ActionResult<object> Checkout([FromBody] GuestCheckoutRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = GuestCheckoutService.CreateGuestOrder(
                    email: request.Email,
                    username: request.Username,
                    street: request.Street,
                    city: request.City,
                    state: request.State,
                    country: request.Country,
                    zip_code: request.ZipCode,
                    orderItems: request.Items,
                    payment_method: request.PaymentMethod,
                    payment_status: request.PaymentStatus,
                    transaction_id: request.TransactionId
                );

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        message = "Failed to create order.",
                        error = result.ErrorMessage
                    });
                }

                return Ok(new
                {
                    message = "Order created successfully!",
                    orderId = result.OrderId,
                    userId = result.UserId,
                    note = "An email has been sent to activate your account if you wish to create a permanent one."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // Request model for the endpoint (simple class - no DTO file needed)
    public class GuestCheckoutRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = "pending";
        public string TransactionId { get; set; } = string.Empty;
    }
}